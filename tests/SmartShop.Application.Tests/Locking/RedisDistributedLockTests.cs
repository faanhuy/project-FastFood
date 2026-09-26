using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using SmartShop.Domain.Common.Exceptions;
using SmartShop.Infrastructure.Locking;
using StackExchange.Redis;
using Xunit;

namespace SmartShop.Application.Tests.Locking;

public class RedisDistributedLockTests
{
    private const string Key = "lock:inventory:store:product";
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(5);

    private readonly Mock<IDatabase> _db = new();
    private readonly RedisDistributedLock _lock;

    public RedisDistributedLockTests()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_db.Object);
        _lock = new RedisDistributedLock(redis.Object, NullLogger<RedisDistributedLock>.Instance);
    }

    private ISetup<IDatabase, Task<bool>> SetupLockTake() =>
        _db.Setup(d => d.LockTakeAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()));

    private ISetup<IDatabase, Task<bool>> SetupLockRelease() =>
        _db.Setup(d => d.LockReleaseAsync(
            It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()));

    [Fact]
    public async Task TryAcquireAsync_LockTaken_ReturnsHandle_AndPassesKeyAndExpiry()
    {
        SetupLockTake().ReturnsAsync(true);

        var handle = await _lock.TryAcquireAsync(Key, Ttl);

        handle.Should().NotBeNull();
        _db.Verify(d => d.LockTakeAsync((RedisKey)Key, It.IsAny<RedisValue>(), Ttl, It.IsAny<CommandFlags>()), Times.Once());
    }

    [Fact]
    public async Task TryAcquireAsync_LockHeldByOther_ReturnsNull_AndNeverReleases()
    {
        SetupLockTake().ReturnsAsync(false);

        var handle = await _lock.TryAcquireAsync(Key, Ttl);

        handle.Should().BeNull();
        _db.Verify(d => d.LockReleaseAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Never());
    }

    [Fact]
    public async Task DisposeAsync_ReleasesWithSameTokenUsedToAcquire()
    {
        RedisValue takenToken = default;
        RedisValue releasedToken = default;
        SetupLockTake()
            .Callback<RedisKey, RedisValue, TimeSpan, CommandFlags>((_, value, _, _) => takenToken = value)
            .ReturnsAsync(true);
        SetupLockRelease()
            .Callback<RedisKey, RedisValue, CommandFlags>((_, value, _) => releasedToken = value)
            .ReturnsAsync(true);

        var handle = await _lock.TryAcquireAsync(Key, Ttl);
        await handle!.DisposeAsync();

        takenToken.IsNullOrEmpty.Should().BeFalse();
        releasedToken.ToString().Should().Be(takenToken.ToString());
        _db.Verify(d => d.LockReleaseAsync((RedisKey)Key, It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Once());
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_ReleasesOnce()
    {
        SetupLockTake().ReturnsAsync(true);
        SetupLockRelease().ReturnsAsync(true);

        var handle = await _lock.TryAcquireAsync(Key, Ttl);
        await handle!.DisposeAsync();
        await handle.DisposeAsync();

        _db.Verify(d => d.LockReleaseAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Once());
    }

    [Fact]
    public async Task TryAcquireAsync_EachCallUsesDifferentToken()
    {
        var tokens = new List<string>();
        SetupLockTake()
            .Callback<RedisKey, RedisValue, TimeSpan, CommandFlags>((_, value, _, _) => tokens.Add(value.ToString()))
            .ReturnsAsync(true);

        await _lock.TryAcquireAsync(Key, Ttl);
        await _lock.TryAcquireAsync(Key, Ttl);

        tokens.Should().HaveCount(2).And.OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task TryAcquireAsync_RedisConnectionFails_ThrowsServiceUnavailable_FailSafe()
    {
        SetupLockTake().ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "redis down"));

        Func<Task> act = () => _lock.TryAcquireAsync(Key, Ttl);

        await act.Should().ThrowAsync<ServiceUnavailableException>();
    }

    [Fact]
    public async Task TryAcquireAsync_RedisTimesOut_ThrowsServiceUnavailable_FailSafe()
    {
        SetupLockTake().ThrowsAsync(new TimeoutException("redis slow"));

        Func<Task> act = () => _lock.TryAcquireAsync(Key, Ttl);

        await act.Should().ThrowAsync<ServiceUnavailableException>();
    }

    [Fact]
    public async Task DisposeAsync_RedisFailsOnRelease_DoesNotThrow()
    {
        SetupLockTake().ReturnsAsync(true);
        SetupLockRelease().ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "redis down"));

        var handle = await _lock.TryAcquireAsync(Key, Ttl);

        Func<Task> act = async () => await handle!.DisposeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task TryAcquireAsync_CancelledToken_ThrowsWithoutCallingRedis()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await _lock.TryAcquireAsync(Key, Ttl, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        _db.Verify(d => d.LockTakeAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()), Times.Never());
    }
}
