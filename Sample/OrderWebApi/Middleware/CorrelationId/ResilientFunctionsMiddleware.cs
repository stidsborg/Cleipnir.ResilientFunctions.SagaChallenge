using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;

namespace OrderWebApi.Middleware.CorrelationId;

public class ResilientFunctionsMiddleware : IPreCreationMiddleware
{
    private readonly CorrelationId _correlationId;

    public ResilientFunctionsMiddleware(CorrelationId correlationId) => _correlationId = correlationId;

    public Task<Result<TResult>> Invoke<TParam, TScrapbook, TResult>(
        TParam param,
        TScrapbook scrapbook,
        Context context,
        Func<TParam, TScrapbook, Context, Task<Result<TResult>>> next
    ) where TParam : notnull where TScrapbook : RScrapbook, new()
    {
        // fill in the blanks...
        
        return next(param, scrapbook, context);
    }

    public Task PreCreation<TParam>(TParam param, Dictionary<string, string> stateDictionary, FunctionId functionId)
        where TParam : notnull
    {
        // fill in the blanks...
        
        return Task.CompletedTask;
    }
}