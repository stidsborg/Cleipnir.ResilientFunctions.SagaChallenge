namespace OrderWebApi.Middleware.CorrelationId;

public class AspNetCorrelationIdMiddleware : IMiddleware
{
    private readonly CorrelationId _correlationId;

    public AspNetCorrelationIdMiddleware(CorrelationId correlationId) => _correlationId = correlationId;

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _correlationId.Value = Guid.NewGuid().ToString();
        return next(context);
    }
}