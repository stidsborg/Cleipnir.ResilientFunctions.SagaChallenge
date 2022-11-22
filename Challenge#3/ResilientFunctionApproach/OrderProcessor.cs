using System.Reactive.Linq;
using Cleipnir.ResilientFunctions.CoreRuntime.Invocation;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Messaging;

namespace SagaChallenge3.ResilientFunctionApproach;

public class OrderProcessor
{
    private readonly MessageBroker _messageBroker;

    public OrderProcessor(MessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    public async Task ProcessOrder(Order order, Scrapbook scrapbook, Context context)
    {
        var eventSource = await context.EventSource;
        
        
    }

    public class Scrapbook : RScrapbook
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
    }
}