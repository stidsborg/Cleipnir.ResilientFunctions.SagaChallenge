using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SagaChallenge.Clients;
using Shouldly;

namespace SagaChallenge.UnitTests;

[TestClass]
public class IdempotentPaymentProviderApiTests
{
    [TestMethod]
    public async Task SameTransactionIdIsUsedAfterCrash()
    {
        var transactionIds = new List<Guid>();
        var scrapbook = new Scrapbook();

        var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
        paymentProviderClientMock
            .Setup(p => p.Reserve(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .Callback<Guid, decimal>((transactionId, _) => transactionIds.Add(transactionId))
            .Throws<TimeoutException>();

        var logisticsClientMock = new Mock<ILogisticsClient>();
        var emailClientMock = new Mock<IEmailClient>();

        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var order = new OrderProcessor.Order("MK-123", customerId, productIds, TotalPrice: 125M);

        var orderProcessor = new OrderProcessor(
            paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
        );

        await Should.ThrowAsync<TimeoutException>(
            () => orderProcessor.ProcessOrder(order, scrapbook)
        );

        transactionIds.Count.ShouldBe(1);
        
        scrapbook = scrapbook.Snapshot ?? new Scrapbook();
        await Should.ThrowAsync<TimeoutException>(
            () => orderProcessor.ProcessOrder(order, scrapbook)
        );
        
        if (transactionIds.Any(id => id == Guid.Empty))
            Assert.Fail("Transaction id must be different to empty Guid");
        
        if (transactionIds.Distinct().Count() != 1)
            Assert.Fail("Different transaction id was used for the same order after a crash");
    }
    
    [TestMethod]
    public async Task TransactionIdToReserveAndCaptureAreIdentical()
    {
        var transactionIds = new List<Guid>();
        var scrapbook = new Scrapbook();

        var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
        paymentProviderClientMock
            .Setup(p => p.Reserve(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .Callback<Guid, decimal>((transactionId, _) => transactionIds.Add(transactionId));
        
        paymentProviderClientMock
            .Setup(p => p.Capture(It.IsAny<Guid>()))
            .Callback<Guid>((transactionId) => transactionIds.Add(transactionId));
        
        var logisticsClientMock = new Mock<ILogisticsClient>();
        var emailClientMock = new Mock<IEmailClient>();

        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var order = new OrderProcessor.Order("MK-123", customerId, productIds, TotalPrice: 125M);

        var orderProcessor = new OrderProcessor(
            paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
        );

        await orderProcessor.ProcessOrder(order, scrapbook);
        if (transactionIds.Distinct().Count() != 1)
            Assert.Fail("Transaction id provided to Reserve and Capture was not identical");
    }
    
    [TestMethod]
    public async Task TransactionIdIsDifferentForTwoDifferentOrders()
    {
        var transactionIds = new List<Guid>();

        var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
        paymentProviderClientMock
            .Setup(p => p.Reserve(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .Callback<Guid, decimal>((transactionId, _) => transactionIds.Add(transactionId));

        var logisticsClientMock = new Mock<ILogisticsClient>();
        var emailClientMock = new Mock<IEmailClient>();

        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var order1 = new OrderProcessor.Order("MK-123", customerId, productIds, TotalPrice: 125M);

        var orderProcessor = new OrderProcessor(
            paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
        );

        await orderProcessor.ProcessOrder(order1, new Scrapbook());
        
        var order2 = new OrderProcessor.Order("MK-124", customerId, productIds, TotalPrice: 125M);
        await orderProcessor.ProcessOrder(order2, new Scrapbook());
       
        if (transactionIds.Distinct().Count() != 2)
            Assert.Fail("Transaction id was identical for two different orders");
    }

    [TestMethod]
    public async Task AllServicesAreCalledInSunshineScenario()
    {
        var scrapbook = new Scrapbook();

        var paymentProviderClientMock = new Mock<IPaymentProviderClient>();
        var logisticsClientMock = new Mock<ILogisticsClient>();
        var emailClientMock = new Mock<IEmailClient>();

        var customerId = Guid.NewGuid();
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var order = new OrderProcessor.Order("MK-123", customerId, productIds, TotalPrice: 125M);

        var orderProcessor = new OrderProcessor(
            paymentProviderClientMock.Object, logisticsClientMock.Object, emailClientMock.Object
        );

        await orderProcessor.ProcessOrder(order, scrapbook);
        
        paymentProviderClientMock.Verify(c => c.Reserve(It.IsAny<Guid>(), It.IsAny<decimal>()));
        paymentProviderClientMock.Verify(c => c.Capture(It.IsAny<Guid>()));
        
        logisticsClientMock.Verify(c => c.ShipProducts(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()));
        emailClientMock.Verify(c => c.SendOrderConfirmation(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()));
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