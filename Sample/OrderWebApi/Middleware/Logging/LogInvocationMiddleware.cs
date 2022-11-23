using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;
using OrderWebApi.OrderProcessor;
using Serilog;
using IMiddleware = Cleipnir.ResilientFunctions.CoreRuntime.Invocation.IMiddleware;

namespace OrderWebApi.Middleware.Logging;

public class LogInvocationMiddleware : IMiddleware
{
    public async Task<Result<TResult>> Invoke<TParam, TScrapbook, TResult>(
        TParam param, 
        TScrapbook scrapbook, 
        Context context, 
        Func<TParam, TScrapbook, Context, Task<Result<TResult>>> next
    ) where TParam : notnull where TScrapbook : RScrapbook, new()
    {
        if (param is not Order order) 
            return await next(param, scrapbook, context);
        
        Log.Logger
            .ForContext<LogInvocationMiddleware>()
            .Information(
                $"ORDER_PROCESSOR_MIDDLEWARE: {(context.InvocationMode == InvocationMode.Direct ? "Processing" : "Reprocessing")} of order '{order.OrderId}' started"
            );

        var result = await next(param, scrapbook, context);

        Log.Logger
            .ForContext<LogInvocationMiddleware>()
            .Information(
                $"ORDER_PROCESSOR_MIDDLEWARE: {(context.InvocationMode == InvocationMode.Direct ? "Processing" : "Reprocessing")} of order '{order.OrderId}' completed"
            );
            
        return result;
    }
}