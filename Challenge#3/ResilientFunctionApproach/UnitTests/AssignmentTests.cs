using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Storage;
using Cleipnir.ResilientFunctions.Testing;

namespace SagaChallenge3.ResilientFunctionApproach.UnitTests;

[TestClass]
public class AssignmentTests
{
    [TestMethod]
    public async Task SunshineScenarioCompletes()
    {
        var functionId = new FunctionId("OrderProcessor", "MK-4321");
        var inMemoryFunctionStore = new InMemoryFunctionStore();
        var testHelper = new TestHelper(inMemoryFunctionStore).For(functionId);
        
        void HandleSentMessage(object sentMessage, MessageBroker messageBroker)
        {
            var response = sentMessage switch
            {
                CancelFundsReservation msg => new FundsReservationCancelled(msg.OrderId),
                CaptureFunds msg => new FundsCaptured(msg.OrderId),
                ReserveFunds msg => new FundsReserved(msg.OrderId),
                SendOrderConfirmationEmail msg => new OrderConfirmationEmailSent(msg.OrderId, msg.CustomerId),
                ShipProducts msg => new ProductsShipped(msg.OrderId),
                _ => default(EventsAndCommands?)
            };

            if (response == null) return;

            testHelper.EventSourceWriter.AppendEvent(response);
        }
        
        var messageBroker = new MessageBroker(HandleSentMessage);
        var orderProcessor = new OrderProcessor(messageBroker);
        var order = new Order(
            "MK-4321",
            CustomerId: Guid.NewGuid(),
            ProductIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            TotalPrice: 124M
        );
        var scrapbook = new OrderProcessor.Scrapbook();

        await orderProcessor.ProcessOrder(order, scrapbook, testHelper.Context);

        var messages = messageBroker.Messages;
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
}