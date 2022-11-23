using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.AspNetCore.Postgres;
using Cleipnir.ResilientFunctions.PostgreSQL;
using OrderWebApi.Middleware.CorrelationId;
using OrderWebApi.Middleware.Logging;
using Serilog;
using Serilog.Events;

namespace OrderWebApi;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var port = args.Any() ? int.Parse(args[0]) : 5000;
        await DatabaseHelper.CreateDatabaseIfNotExists(Settings.ConnectionString);
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        
        // Add services to the container.
        OrderProcessor.IoCBindings.AddBindings(builder.Services);
        
        //add this line register Resilient Functions' dependencies
        builder.Services.UseResilientFunctions( 
            Settings.ConnectionString,
            _ =>
            {
                var options = new Options(
                    unhandledExceptionHandler: rfe => Log.Logger.Error(rfe, "ResilientFrameworkException occured"),
                    crashedCheckFrequency: TimeSpan.FromSeconds(1)
                );
                //uncomment to enable logging middleware: options.UseMiddleware(new LogInvocationMiddleware());
                //uncomment to enable correlationid middleware: options.UseMiddleware<Middleware.CorrelationId.ResilientFunctionsMiddleware>();
                return options;
            });
        
        builder.Host.UseSerilog();
        builder.Services.AddControllers();
        builder.Services.AddCorrelationIdMiddleware();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseMiddleware<AspNetCorrelationIdMiddleware>();
        
        app.MapControllers();

        await app.RunAsync($"http://localhost:{port}");
    }
}