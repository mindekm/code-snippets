public static class Program
{
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        TaskScheduler.UnobservedTaskException += (_, eventArgs)
            => Log.Error(eventArgs.Exception, "UnobservedTaskException has been thrown");

        try
        {
            WebApplication
                .CreateBuilder()
                .ConfigureSerilog()
                .Build()
                .Run();

            return 0;
        }
        catch()
        {
            Log.Fatal(e, "Host terminated unexpectedly")
            return 1
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, configuration) => 
        {
            configuration
                .ReadFrom.Configuration(context.Configuration);
        });

        return builder;
    }
}