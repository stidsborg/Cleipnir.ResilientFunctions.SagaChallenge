using Cleipnir.ResilientFunctions.Domain;
using SagaChallenge2.Clients;

namespace SagaChallenge2;

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
        var orderBrand = Brand.Unknown;
        await _paymentProviderClient.Reserve(scrapbook.TransactionId, order.TotalPrice);
        await _logisticsClient.ShipProducts(order.OrderId, order.CustomerId, orderBrand, order.ProductIds);
        await _paymentProviderClient.Capture(scrapbook.TransactionId);
        await _emailClient.SendOrderConfirmation(order.CustomerId, orderBrand, order.ProductIds);
    }
    
    public static object GetOrderWithBrand(string orderId, Brand brand, Guid customerId, IEnumerable<Guid> productIds, decimal totalPrice)
        => throw new NotImplementedException(); //todo implement this method to allow tests complete

    public record Order(string OrderId, Guid CustomerId, IEnumerable<Guid> ProductIds, decimal TotalPrice);

    public class Scrapbook : RScrapbook
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
    }
}