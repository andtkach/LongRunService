namespace LongRunService.Processing;

public class MessageProcessor : IMessageProcessor
    {
        private readonly ILogger<MessageProcessor> _logger;

	public MessageProcessor(ILogger<MessageProcessor> logger)
        {
	        _logger = logger;
        }

        public async Task ProcessMessageAsync(
			string message,
			CancellationToken cancellationToken = default)
        {
			_logger.LogInformation("Processing {Message} at {Now}", message, DateTime.UtcNow);
			await Task.Delay(1000, cancellationToken);
	}
    }
