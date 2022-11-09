using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SagaChallenge2.Clients;

namespace SagaChallenge2.UnitTests;

[TestClass]
public class Assigment1Tests
{
    [TestMethod]
    public async Task OrdersWithBrandAreProcessedCorrectly()
    {
        foreach (Brand brand in Enum.GetValues(typeof(Brand)))
        {
            var scrapbook = new Scrapbook();
            await scrapbook.Save();

            var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
            var logisticsClientMock = new Mock<ILogisticsClient>();
            var emailClientMock = new Mock<IEmailClient>();
            var customerId = Guid.NewGuid();
            var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var order = OrderProcessor.GetOrderWithBrand("MK-123", brand, customerId, productIds, totalPrice: 125M);

            var processOrderMethodInfo = typeof(OrderProcessor).GetMethod($"{nameof(OrderProcessor.ProcessOrder)}");
            var orderProcessor = new OrderProcessor(
                paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
            );

            await (Task)processOrderMethodInfo!.Invoke(orderProcessor, new[] { order, scrapbook })!;

            logisticsClientMock
                .Verify(c => c.ShipProducts("MK-123", customerId, brand, productIds), Times.Once);
            paymentProviderClientMock
                .Verify(p => p.Capture(It.IsAny<Guid>()), Times.Once);
            paymentProviderClientMock
                .Verify(p => p.Reserve(It.IsAny<Guid>(), 125M), Times.Once);
            emailClientMock
                .Verify(e => e.SendOrderConfirmation(customerId, brand, productIds));
        }
    }
    
    [TestMethod]
    public async Task PreviousOrderWithoutBrandIsProcessedCorrectly()
    {
            var scrapbook = new Scrapbook();
            await scrapbook.Save();

            var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
            var logisticsClientMock = new Mock<ILogisticsClient>();
            var emailClientMock = new Mock<IEmailClient>();

            var orderId = Guid.NewGuid().ToString();
            var customerId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            const decimal totalPrice = 123M;
            
            var json = @"{""OrderId"":""ORDER_ID"", ""CustomerId"":""CUSTOMER_ID"",""ProductIds"":[""PRODUCT_ID1"",""PRODUCT_ID2""], ""TotalPrice"": TOTAL_PRICE}"
                .Replace("ORDER_ID", orderId)
                .Replace("CUSTOMER_ID", customerId.ToString())
                .Replace("PRODUCT_ID1", productId1.ToString())
                .Replace("PRODUCT_ID2", productId2.ToString())
                .Replace("TOTAL_PRICE", totalPrice.ToString("G"));
            
            var oldOrder = JsonSerializer.Deserialize<OrderProcessor.Order>(json)!;

            var processOrderMethodInfo = typeof(OrderProcessor).GetMethod($"{nameof(OrderProcessor.ProcessOrder)}");
            var orderProcessor = new OrderProcessor(
                paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
            );

            await (Task) processOrderMethodInfo!.Invoke(orderProcessor, new object[] { oldOrder, scrapbook })!;

            var products = (IEnumerable<Guid>)new[] { productId1, productId2 };
            
            logisticsClientMock.Verify(c =>
                c.ShipProducts(
                    orderId,
                    customerId,
                    Brand.AbcLavpris,
                    products
                ), Times.Exactly(1)
            );
            paymentProviderClientMock
                .Verify(p => p.Capture(It.IsAny<Guid>()), Times.Once);
            paymentProviderClientMock
                .Verify(p => p.Reserve(It.IsAny<Guid>(), totalPrice), Times.Once);
            emailClientMock
                .Verify(e => e.SendOrderConfirmation(customerId, Brand.AbcLavpris, products));
    }

    private class Scrapbook : OrderProcessor.Scrapbook
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public Scrapbook? Snapshot { get; private set; }
        
        public override Task Save()
        {
            var json = JsonSerializer.Serialize(this);
            Snapshot = JsonSerializer.Deserialize<Scrapbook>(json)!;
            return Task.CompletedTask;
        }
    }
}