using Serilog;
using Threads.Api.Startup;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Starting Threads.Api");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();
    app.UseMiddlewares();
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
