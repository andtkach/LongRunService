namespace LongRunService.Processing;

public interface IMessageProcessor
    {
        Task ProcessMessageAsync(
			string message,
			CancellationToken cancellationToken = default);
    }
