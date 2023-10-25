using FluentAssertions;
using LongRunService.BackgroundServices;
using Microsoft.Extensions.Logging.Abstractions;

namespace LongRunService.Tests;

public class QueueReadingServiceTests
{
	[Fact]
	public async Task ShouldSwallowExceptions_AndCompleteWriter()
	{
		// Arrange

		using var sut = new WorkerService2(
			NullLogger<WorkerService2>.Instance);

		// Act
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		await sut.StartAsync(default);

		// Assert
		var act = async () => { await sut.StopAsync(cts.Token); };
		await act.Should().NotThrowAsync();
		true.Should().Be(true);
	}

	[Fact]
	public async Task ShouldStopWithoutException_WhenCancelled()
	{
		var sut = new WorkerService2(
			NullLogger<WorkerService2>.Instance);

		await sut.StartAsync(default);

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

		var act = async () => { await sut.StopAsync(cts.Token); };
		await act.Should().NotThrowAsync();
		true.Should().Be(true);
	}
}
