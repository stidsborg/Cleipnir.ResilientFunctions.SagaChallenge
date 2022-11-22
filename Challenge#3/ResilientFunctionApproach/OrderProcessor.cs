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

        _messageBroker.Send(new ReserveFunds(order.OrderId, order.TotalPrice, scrapbook.TransactionId, order.CustomerId));
        await eventSource.All.OfType<FundsReserved>().NextEvent(maxWaitMs: 5_000);

        _messageBroker.Send(new ShipProducts(order.OrderId, order.CustomerId, order.ProductIds));
        await eventSource.All.OfType<ProductsShipped>().NextEvent(maxWaitMs: 5_000);

        _messageBroker.Send(new CaptureFunds(order.OrderId, order.CustomerId, scrapbook.TransactionId));
        await eventSource.All.OfType<FundsCaptured>().NextEvent(maxWaitMs: 5_000);

        _messageBroker.Send(new SendOrderConfirmationEmail(order.OrderId, order.CustomerId));
        await eventSource.All.OfType<OrderConfirmationEmailSent>().NextEvent(maxWaitMs: 5_000);
    }

    public class Scrapbook : RScrapbook
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
    }
}