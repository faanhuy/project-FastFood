using FluentAssertions;
using SmartShop.Infrastructure.Locking;
using Xunit;

namespace SmartShop.Application.Tests.Locking;

public class InMemoryDistributedLockTests
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(5);

    private readonly ManualTimeProvider _time = new();
    private readonly InMemoryDistributedLock _lock;

    public InMemoryDistributedLockTests() => _lock = new InMemoryDistributedLock(_time);

    [Fact]
    public async Task TryAcquireAsync_FreeKey_ReturnsHandle()
    {
        var handle = await _lock.TryAcquireAsync("lock:a", Ttl);

        handle.Should().NotBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_HeldKey_ReturnsNull()
    {
        await _lock.TryAcquireAsync("lock:a", Ttl);

        var second = await _lock.TryAcquireAsync("lock:a", Ttl);

        second.Should().BeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_DifferentKeys_AreIndependent()
    {
        await _lock.TryAcquireAsync("lock:a", Ttl);

        var other = await _lock.TryAcquireAsync("lock:b", Ttl);

        other.Should().NotBeNull();
    }

    [Fact]
    public async Task DisposeAsync_ReleasesKey_SoItCanBeAcquiredAgain()
    {
        var first = await _lock.TryAcquireAsync("lock:a", Ttl);

        await first!.DisposeAsync();

        (await _lock.TryAcquireAsync("lock:a", Ttl)).Should().NotBeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_BeforeExpiry_StillHeld()
    {
        await _lock.TryAcquireAsync("lock:a", Ttl);
        _time.Advance(TimeSpan.FromSeconds(4));

        (await _lock.TryAcquireAsync("lock:a", Ttl)).Should().BeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_AfterExpiry_TakesOverLock()
    {
        await _lock.TryAcquireAsync("lock:a", Ttl);
        _time.Advance(Ttl + TimeSpan.FromMilliseconds(1));

        (await _lock.TryAcquireAsync("lock:a", Ttl)).Should().NotBeNull();
    }

    [Fact]
    public async Task DisposeAsync_StaleHandle_DoesNotReleaseNewOwnersLock()
    {
        // A giữ khóa → hết TTL → B giành khóa → A (chậm) mới nhả: không được xóa khóa của B.
        var a = await _lock.TryAcquireAsync("lock:a", Ttl);
        _time.Advance(Ttl + TimeSpan.FromMilliseconds(1));
        var b = await _lock.TryAcquireAsync("lock:a", Ttl);
        b.Should().NotBeNull();

        await a!.DisposeAsync();

        (await _lock.TryAcquireAsync("lock:a", Ttl)).Should().BeNull("khóa của B vẫn còn hiệu lực");
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_ReleasesOnlyOnce()
    {
        var a = await _lock.TryAcquireAsync("lock:a", Ttl);
        await a!.DisposeAsync();
        var b = await _lock.TryAcquireAsync("lock:a", Ttl);
        b.Should().NotBeNull();

        await a.DisposeAsync(); // lần 2: không được xóa khóa của B

        (await _lock.TryAcquireAsync("lock:a", Ttl)).Should().BeNull();
    }

    [Fact]
    public async Task TryAcquireAsync_CancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = async () => await _lock.TryAcquireAsync("lock:a", Ttl, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TryAcquireAsync_ManyConcurrentCallers_OnlyOneWins()
    {
        var results = await Task.WhenAll(
            Enumerable.Range(0, 64).Select(_ => Task.Run(() => _lock.TryAcquireAsync("lock:hot", Ttl))));

        results.Count(r => r is not null).Should().Be(1);
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 9, 27, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
