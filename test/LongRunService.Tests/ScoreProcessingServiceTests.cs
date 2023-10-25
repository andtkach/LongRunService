using FluentAssertions;
using LongRunService.BackgroundServices;
using LongRunService.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LongRunService.Tests;

public class ScoreProcessingServiceTests
{
	[Fact]
	public async Task ShouldStopApplication_WhenExceptionThrown_BecauseServiceProviderDoesNotContainRequiredService()
	{
		var sp = new ServiceCollection().BuildServiceProvider();

		var hostApplicationLifetime = new Mock<IHostApplicationLifetime>();
		
		var sut = new WorkerService1(
			NullLogger<WorkerService1>.Instance,
			sp,
			hostApplicationLifetime.Object);

		await sut.StartAsync(default);
		if (sut.ExecuteTask != null) await sut.ExecuteTask;

		hostApplicationLifetime.Verify(x => x.StopApplication(), Times.Once);
	}

	[Fact]
	public async Task ExecuteAsync_ShouldStopWithoutException_WhenCancelled()
	{
		var sc = new ServiceCollection();
		sc.AddTransient<IMessageProcessor, FakeScoreProcessor>();
		var sp = sc.BuildServiceProvider();

		var sut = new WorkerService1(
			NullLogger<WorkerService1>.Instance,
			sp,
			Mock.Of<IHostApplicationLifetime>());

		await sut.StartAsync(default);

		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
		await sut.StopAsync(cts.Token);
		if (sut.ExecuteTask != null)
		{
			await sut.ExecuteTask;

			sut.ExecuteTask.IsCompletedSuccessfully.Should().BeTrue();
		}
	}

	[Fact]
	public async Task ShouldCallScoreProcessor_ForEachMessageInChannel()
	{
		var scoreProcessor = new FakeScoreProcessor();
		var sc = new ServiceCollection();
		sc.AddTransient<IMessageProcessor>(s => scoreProcessor);

		var sp = sc.BuildServiceProvider();

		var sut = new WorkerService1(
			NullLogger<WorkerService1>.Instance,
			sp,
			Mock.Of<IHostApplicationLifetime>());

		await sut.StartAsync(default);
		if (sut.ExecuteTask != null) await sut.ExecuteTask;

		scoreProcessor.ExecutionCount.Should().Be(2);
	}

	[Fact]
	public async Task ShouldSwallowExceptions_AndStopApplication()
	{
		var sc = new ServiceCollection();

		var scoreProcessor = new Mock<IMessageProcessor>();
		scoreProcessor.Setup(x => x
			.ProcessMessageAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("My exception"));

		sc.AddTransient(s => scoreProcessor);

		var sp = sc.BuildServiceProvider();

		var hostAppLifetime = new Mock<IHostApplicationLifetime>();

		var sut = new WorkerService1(
			NullLogger<WorkerService1>.Instance,
			sp,
			hostAppLifetime.Object);

		await sut.StartAsync(default);
		if (sut.ExecuteTask != null) await sut.ExecuteTask;

		hostAppLifetime.Verify(x => x.StopApplication(), Times.AtLeastOnce);
	}

	private class FakeScoreProcessor : IMessageProcessor
	{
		public int ExecutionCount;

		public Task ProcessMessageAsync(string message,
			CancellationToken cancellationToken = default)
		{
			ExecutionCount++;
			return Task.CompletedTask;
		}
	}
}
