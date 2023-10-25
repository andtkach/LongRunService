global using Microsoft.Extensions.Options;
global using System.Text.Json;
using LongRunService.BackgroundServices;
using LongRunService.Processing;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureServices((hostContext, services) =>
	{
		services.Configure<HostOptions>(hostOptions =>
		{
			hostOptions.BackgroundServiceExceptionBehavior =
				BackgroundServiceExceptionBehavior.Ignore;
			hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(60);
		});
		
		services.AddTransient<IMessageProcessor, MessageProcessor>();
		services.AddHostedService<WorkerService1>();
		services.AddHostedService<WorkerService2>();		
	})
	.Build();

await host.RunAsync();
