namespace SagaChallenge3.StateMachineApproach.UnitTests;

[TestClass]
public class AssignmentTests
{
    [TestMethod]
    public void SunshineScenarioCompletes()
    {
        VerifyOrderProcessorIsAbleToHandleAllMessageTypes();
        
        var order = new Order(
            "MK-4321",
            CustomerId: Guid.NewGuid(),
            ProductIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            TotalPrice: 124M
        );
        
        var messages = CompleteOrderProcessing(order);
        VerifyCommandMessageOrder(messages);
        
        var reserveFunds = (ReserveFunds?) messages.SingleOrDefault(e => e is ReserveFunds);
        if (reserveFunds == null) 
            Assert.Fail("ReserveFunds message was not sent");
        else
        {
            Assert.AreEqual(124M, reserveFunds.Amount, "ReserveFunds amount was not as provided in order");
            Assert.AreEqual(order.CustomerId, reserveFunds.CustomerId, "ReserveFunds' customer id was not as provided in order");   
        }

        var shipProducts = (ShipProducts?) messages.SingleOrDefault(e => e is ShipProducts);
        if (shipProducts == null) 
            Assert.Fail("ReserveFunds message was not sent");
        else
            Assert.AreEqual(order.CustomerId, shipProducts.CustomerId, "ShipProducts's customer id was not as provided in order");

        var captureFunds = (CaptureFunds?) messages.SingleOrDefault(e => e is CaptureFunds);
        if (captureFunds == null) Assert.Fail("CaptureFunds message was not sent");
        else
        {
            Assert.AreEqual(order.CustomerId, captureFunds.CustomerId, "CaptureFunds' customer id was not as provided in order");
            Assert.AreEqual(reserveFunds.TransactionId, captureFunds.TransactionId, "TransactionId not the same in ReserveFunds and CaptureFunds");
        }
        
        var sendOrderConfirmationEmail = (SendOrderConfirmationEmail?) messages.SingleOrDefault(e => e is SendOrderConfirmationEmail);
        if (sendOrderConfirmationEmail == null) 
            Assert.Fail("CaptureFunds message was not sent");
        else
            Assert.AreEqual(order.CustomerId, sendOrderConfirmationEmail.CustomerId, "SendOrderConfirmationEmail's customer id was not as provided in order");
    }

    private static void VerifyCommandMessageOrder(IEnumerable<object> events)
    {
        var commands = events
            .Where(e => e is ReserveFunds or ShipProducts or CaptureFunds or SendOrderConfirmationEmail)
            .ToList();
        
        if (commands.Count >= 1)
            Assert.IsTrue(commands[0] is ReserveFunds, "Expected ReserveFunds to be first command sent");
        if (commands.Count >= 2)
            Assert.IsTrue(commands[1] is ShipProducts, "Expected ShipProducts to be second command sent");
        if (commands.Count >= 3)
            Assert.IsTrue(commands[2] is CaptureFunds, "Expected CaptureFunds to be third command sent");
        if (commands.Count >= 4)
            Assert.IsTrue(commands[3] is SendOrderConfirmationEmail, "Expected SendOrderConfirmationEmail to be fourth command sent");
    }

    private static List<object> CompleteOrderProcessing(Order order)
    {
        var events = new List<object>();
        var outgoing = new List<object>();
        for (var i = 0; i < 50; i++)
        {
            var sent = new List<object>();
            var messageBroker = new MessageBroker((e, _) => sent.Add(e));

            var orderProcessor = new OrderProcessor(messageBroker, order);
            var methods = typeof(OrderProcessor).GetMethods().Where(m => m.Name == "Handle").ToArray();

            foreach (var @event in events)
            {
                var methodInfo = methods.SingleOrDefault(m => m.GetParameters().Single().ParameterType == @event.GetType());
                methodInfo?.Invoke(orderProcessor, new[] { @event });
            }

            orderProcessor.ExecuteNextStep();
            
            foreach (var sentCommand in sent)
            {
                var response = HandleSentMessage(sentCommand);
                if (response != null)
                    events.Add(response);
                else
                    events.Add(sentCommand);
            }
            outgoing.AddRange(sent);

            if (orderProcessor.Completed)
                return outgoing;
        }
        
        Assert.Fail("Order Processor did not finish in a timely manner");
        return new List<object>(); //will never happen as line above throws exception
    }

    private static object? HandleSentMessage(object sentMessage)
    {
        var response = sentMessage switch
        {
            CancelFundsReservation msg => new FundsReservationCancelled(msg.OrderId),
            CaptureFunds msg => new FundsCaptured(msg.OrderId),
            ReserveFunds msg => new FundsReserved(msg.OrderId),
            SendOrderConfirmationEmail msg => new OrderConfirmationEmailSent(msg.OrderId, msg.CustomerId),
            ShipProducts msg => new ProductsShipped(msg.OrderId),
            _ => default(object)
        };

        return response;
    }

    private static void VerifyOrderProcessorIsAbleToHandleAllMessageTypes()
    {
        var orderProcessor = new OrderProcessor(new MessageBroker((_, _) => { }), order: null!);
        if (orderProcessor is not IHandleMessage<FundsReserved>)
            Assert.Fail($"OrderProcessor must handle {nameof(FundsReserved)} message");
        if (orderProcessor is not IHandleMessage<ProductsShipped>)
            Assert.Fail($"OrderProcessor must handle {nameof(ProductsShipped)} message");
        if (orderProcessor is not IHandleMessage<FundsCaptured>)
            Assert.Fail($"OrderProcessor must handle {nameof(FundsCaptured)} message");
        if (orderProcessor is not IHandleMessage<OrderConfirmationEmailSent>)
            Assert.Fail($"OrderProcessor must handle {nameof(OrderConfirmationEmailSent)} message");   
    }
}