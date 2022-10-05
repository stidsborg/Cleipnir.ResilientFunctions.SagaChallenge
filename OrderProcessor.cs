using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cleipnir.ResilientFunctions.Domain;
using SagaChallenge.Clients;

namespace SagaChallenge;

public class OrderProcessor
{
    private readonly IPaymentProviderClient _paymentProviderClient;
    private readonly IEmailClient _emailClient;
    private readonly ILogisticsClient _logisticsClient;

    public OrderProcessor(IPaymentProviderClient paymentProviderClient, ILogisticsClient logisticsClient, IEmailClient emailClient)
    {
        _paymentProviderClient = paymentProviderClient;
        _logisticsClient = logisticsClient;
        _emailClient = emailClient;
    }

    public async Task ProcessOrder(Order order, Scrapbook scrapbook)
    {
        //welcome to the cleipnir saga challenge!
        
        //hint:
        //you can add properties to the scrapbook to help re-instate the same state after a crash and subsequent retry  
        //invoke scrapbook.Save() to persist current state
        
        //assignment 1: ensure the payment provider api uses same transaction id for the same order
        //assignment 2: ensure the ship products api is invoked at most once.
        //              thrown an exception if it is about to be invoked a second time.

        var transactionId = Guid.Empty;
        await _paymentProviderClient.Reserve(transactionId, order.TotalPrice);
        await _logisticsClient.ShipProducts(order.CustomerId, order.ProductIds);
        await _paymentProviderClient.Capture(transactionId);
        await _emailClient.SendOrderConfirmation(order.CustomerId, order.ProductIds);
    }

    public record Order(string OrderId, Guid CustomerId, IEnumerable<Guid> ProductIds, decimal TotalPrice);

    public class Scrapbook : RScrapbook
    {
        // add custom properties here
    }
}