namespace OrderWebApi.Middleware.CorrelationId;

public static class IoCBindings
{
    public static void AddCorrelationIdMiddleware(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<CorrelationId>();
        serviceCollection.AddSingleton<AspNetCorrelationIdMiddleware>();
        serviceCollection.AddSingleton<ResilientFunctionsMiddleware>();
    }
}