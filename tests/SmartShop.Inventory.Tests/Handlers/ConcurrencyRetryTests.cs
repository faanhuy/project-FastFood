using FluentAssertions;
using SmartShop.Inventory.Application.Common;
using SmartShop.Inventory.Domain.Common.Exceptions;
using Xunit;

namespace SmartShop.Inventory.Tests.Handlers;

public class ConcurrencyRetryTests
{
    [Fact]
    public async Task RunAsync_SucceedsFirstTime_RunsExactlyOnce()
    {
        var calls = 0;

        var result = await ConcurrencyRetry.RunAsync(() =>
        {
            calls++;
            return Task.FromResult(42);
        });

        result.Should().Be(42);
        calls.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_ConflictThenSuccess_RetriesOnceAndReturnsResult()
    {
        var calls = 0;

        var result = await ConcurrencyRetry.RunAsync(() =>
        {
            calls++;
            if (calls == 1) throw new ConcurrencyException("xmin changed");
            return Task.FromResult("ok");
        });

        result.Should().Be("ok");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task RunAsync_ConflictOnEveryAttempt_RethrowsAfterMaxAttempts()
    {
        var calls = 0;

        var act = () => ConcurrencyRetry.RunAsync<int>(() =>
        {
            calls++;
            throw new ConcurrencyException("xmin changed");
        });

        await act.Should().ThrowAsync<ConcurrencyException>();
        calls.Should().Be(2);   // mặc định maxAttempts = 2: 1 lần đầu + 1 lần thử lại
    }

    [Fact]
    public async Task RunAsync_CustomMaxAttempts_IsRespected()
    {
        var calls = 0;

        var act = () => ConcurrencyRetry.RunAsync<int>(() =>
        {
            calls++;
            throw new ConcurrencyException("xmin changed");
        }, maxAttempts: 4);

        await act.Should().ThrowAsync<ConcurrencyException>();
        calls.Should().Be(4);
    }

    [Fact]
    public async Task RunAsync_OtherExceptions_AreNotRetried()
    {
        var calls = 0;

        var act = () => ConcurrencyRetry.RunAsync<int>(() =>
        {
            calls++;
            throw new InvalidOperationException("boom");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
        calls.Should().Be(1);
    }
}
