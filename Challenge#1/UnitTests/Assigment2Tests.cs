using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SagaChallenge.Clients;

namespace SagaChallenge.UnitTests;

[TestClass]
public class Assigment2Tests
{
    [TestMethod]
    public async Task LogisticsServiceIsOnlyInvokedOnceInSunshineScenario()
    {
        var shipProductsInvocations = 0;
        var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
        var logisticsClientMock = new Mock<ILogisticsClient>();
        logisticsClientMock
            .Setup(c => c.ShipProducts(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
            .Callback<Guid, IEnumerable<Guid>>((_, _) => shipProductsInvocations++);
        var emailClientMock = new Mock<IEmailClient>();

        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var order = new OrderProcessor.Order("MK-123", customerId, productIds, TotalPrice: 125M);

        var orderProcessor = new OrderProcessor(
            paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
        );

        await orderProcessor.ProcessOrder(order, new Scrapbook());

        if (shipProductsInvocations == 0)
            Assert.Fail("LogisticsService was never invoked");
        if (shipProductsInvocations != 1)
            Assert.Fail("LogisticsService was invoked multiple times");
    }
    
    [TestMethod]
    public async Task LogisticsServiceIsOnlyInvokedOnceAfterCrash()
    {
        Scrapbook? scrapbookWhenShippingProducts = null;
        var scrapbook = new Scrapbook();
        var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
        var logisticsClientMock = new Mock<ILogisticsClient>();
        logisticsClientMock
            .Setup(
            c => c.ShipProducts(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>())
            )
            .Callback<Guid, IEnumerable<Guid>>((_, _) => scrapbookWhenShippingProducts ??= (scrapbook.Snapshot ?? new Scrapbook()))
            .Throws<TimeoutException>();
        
        var emailClientMock = new Mock<IEmailClient>();

        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var order = new OrderProcessor.Order("MK-123", customerId, productIds, TotalPrice: 125M);

        var orderProcessor = new OrderProcessor(
            paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
        );

        Exception? thrownException = null;
        try
        {
            await orderProcessor.ProcessOrder(order, scrapbook);
        }
        catch (Exception exception)
        {
            thrownException = exception;
        }
        if (thrownException == null)
            Assert.Fail("Process order invocation was expected to throw exception when logistics service invocation fails");

        var shipProductsInvoked = false;
        logisticsClientMock = new Mock<ILogisticsClient>();
        logisticsClientMock
            .Setup(c => c.ShipProducts(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
            .Callback<Guid, IEnumerable<Guid>>((_, _) => shipProductsInvoked = true);

        orderProcessor = new OrderProcessor(paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object);
        var exceptionWasThrown = false;
        try
        {
            await orderProcessor.ProcessOrder(order, scrapbookWhenShippingProducts!);
        }
        catch (Exception)
        {
            exceptionWasThrown = true;
        }
        if (shipProductsInvoked)
            Assert.Fail("ShipProducts was invoked again after crash");
        
        if (!exceptionWasThrown)
            Assert.Fail("Expected subsequent order processing invocation to fail when logistics service timed-out in previous invocation");
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