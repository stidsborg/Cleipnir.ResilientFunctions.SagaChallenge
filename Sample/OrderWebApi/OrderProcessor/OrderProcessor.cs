using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.AspNetCore.Core;
using Cleipnir.ResilientFunctions.Domain;
using Serilog;

namespace OrderWebApi.OrderProcessor;

public class OrderProcessor : IRegisterRFuncOnInstantiation
{
    public RAction.Invoke<Order, RScrapbook> ProcessOrder { get; }
    public ControlPanels<Order, RScrapbook> ControlPanels { get; }

    public OrderProcessor(RFunctions rFunctions)
    {
        var registration = rFunctions
            .RegisterMethod<Inner>()
            .RegisterAction<Order>(
                nameof(OrderProcessor),
                inner => inner.ProcessOrder
            );

        ProcessOrder = registration.Invoke;
        ControlPanels = registration.ControlPanels;
    }

    public class Inner
    {
        private readonly IPaymentProviderClient _paymentProviderClient;
        private readonly IEmailClient _emailClient;
        private readonly ILogisticsClient _logisticsClient;

        public Inner(IPaymentProviderClient paymentProviderClient, IEmailClient emailClient, ILogisticsClient logisticsClient)
        {
            _paymentProviderClient = paymentProviderClient;
            _emailClient = emailClient;
            _logisticsClient = logisticsClient;
        }

        public async Task ProcessOrder(Order order)
        {
            var transactionId = Guid.NewGuid();
            await _paymentProviderClient.Reserve(order.CustomerId, transactionId, order.TotalPrice);
            await _logisticsClient.ShipProducts(order.CustomerId, order.ProductIds);
            await _paymentProviderClient.Capture(transactionId);
            await _emailClient.SendOrderConfirmation(order.CustomerId, order.ProductIds);
        }        
    }
}