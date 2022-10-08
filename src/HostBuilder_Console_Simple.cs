var builder = Host
    .CreateDefaultBuilder()
    .ConfigureServices(s =>
    {
        s.AddTransient<Application>();
    })
    .Build();

using var scope = builder.Services.CreateScope();
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

await scope.ServiceProvider.GetRequiredService<Application>().Run(cts.Token);

public sealed class Application
{
    private readonly ILogger logger;

    public Application(ILogger logger)
    {
        this.logger = logger;
    }

    public Task Run(CancellationToken cancellationToken)
    {
        logger.LogInformation("Hello World");

        return Task.CompletedTask;
    }
}