using System.Diagnostics;
using LongRunService.Logging;
using LongRunService.Processing;

namespace LongRunService.BackgroundServices;

public class WorkerService1 : BackgroundService
{
	private readonly ILogger<WorkerService1> _logger;
	private readonly IServiceProvider _serviceProvider;
	private readonly IHostApplicationLifetime _hostApplicationLifetime;

	public WorkerService1(
		ILogger<WorkerService1> logger,
		IServiceProvider serviceProvider,
		IHostApplicationLifetime hostApplicationLifetime
	   )
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
		_hostApplicationLifetime = hostApplicationLifetime;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();

		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Do processing...");
				using var scope = _serviceProvider.CreateScope();
				var scoreProcessor = scope.ServiceProvider.GetRequiredService<IMessageProcessor>();
				var message = "Hello";
				await scoreProcessor.ProcessMessageAsync(message, stoppingToken);
				_logger.LogInformation("Finished processing iteration");
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}

			_logger.LogInformation("Finished processing");
		}
		catch (OperationCanceledException)
		{
			_logger.OperationCancelledExceptionOccurred();
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "An unhandled exception was thrown");
		}
		finally
		{
			_hostApplicationLifetime.StopApplication();
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		var sw = Stopwatch.StartNew();

		await base.StopAsync(cancellationToken);

		_logger.LogInformation("Completed shutdown in {Ms}ms", sw.ElapsedMilliseconds);
	}
}
