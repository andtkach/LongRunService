using LongRunService.Logging;

namespace LongRunService.BackgroundServices;

public class WorkerService2 : BackgroundService
{
	private readonly ILogger<WorkerService2> _logger;
	
	public long Counter { get; private set; }

	public WorkerService2(
		ILogger<WorkerService2> logger
		)
	{
		_logger = logger;
		Counter = 1;
		_logger.LogInformation("Counter service created from {Counter}", Counter);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();

		_logger.LogInformation("Started counter service.");

		stoppingToken.Register(() =>
		{
			_logger.LogInformation("Ending counter service due to host shutdown");
		});

		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				Counter++;
				_logger.LogInformation("Count {Counter}", Counter);
				await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
			}
		}
		catch (OperationCanceledException)
		{
			_logger.OperationCancelledExceptionOccurred();
		}
		catch (Exception ex)
		{
			_logger.LogCritical(ex, "A critical exception was thrown in Counter service.");
		}
		finally
		{
			Counter = 0;
		}
	}
}
