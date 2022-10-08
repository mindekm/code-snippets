await Host
    .CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddHostedService<ConsoleHostedService>();
    })
    .RunConsoleAsync();

public sealed class ConsoleHostedService : IHostedService
{
    private readonly ILogger logger;
    private readonly IHostApplicationLifetime lifetime;

    public ConsoleHostedService(ILogger logger, IHostApplicationLifetime lifetime)
    {
        this.logger = logger;
        this.lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    await Run(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex, "Unhandled exception!");
                }
                finally
                {
                    lifetime.StopApplication();
                }
            }, cancellationToken);
        });

        return Task.CompletedTask;
    }

    private Task Run(CancellationToken cancellationToken)
    {
        logger.Information("Hello World");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}    