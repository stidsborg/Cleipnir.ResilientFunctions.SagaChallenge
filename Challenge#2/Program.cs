using System.Text.Json;
using Cleipnir.ResilientFunctions;
using Cleipnir.ResilientFunctions.Domain;
using Cleipnir.ResilientFunctions.Helpers;
using Cleipnir.ResilientFunctions.Storage;
using SagaChallenge2.Clients;
using Serilog;

namespace SagaChallenge2;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
        
        var store = new InMemoryFunctionStore();
        var rFunctions = new RFunctions(
            store,
            new Settings(
                UnhandledExceptionHandler: Console.WriteLine,
                CrashedCheckFrequency: TimeSpan.FromMilliseconds(100)
            )
        );

        var orderProcessor = new OrderProcessor(
            new PaymentProviderClientStub(),
            new LogisticsClientStub(),
            new EmailClientStub()
        );
        var scheduleRAction = rFunctions.RegisterAction<OrderProcessor.Order, OrderProcessor.Scrapbook>(
            nameof(OrderProcessor),
            orderProcessor.ProcessOrder
        ).Schedule;

        while (true)
        {
            Console.WriteLine("Press enter to start processing of order: (1) New order (2) Old order");
            var read = Console.ReadLine();
            if (read == "1") 
                await CreateNewOrder(scheduleRAction);
            else if (read == "2") 
                await CreateNewOldOrder(store);
        }
    }

    private static Task CreateNewOrder(Schedule<OrderProcessor.Order, OrderProcessor.Scrapbook> scheduleRAction)
    {
        var order = new OrderProcessor.Order(
            OrderId: Random.Shared.Next(1000, 9999).ToString(),
            CustomerId: Guid.NewGuid(),
            ProductIds: new[] { Guid.NewGuid(), Guid.NewGuid() },
            TotalPrice: 120
        );
        
        Log.Logger.Information("Start processing of new order with order number {orderId}", order.OrderId);
        return scheduleRAction(order.OrderId, order);
    }

    private static async Task CreateNewOldOrder(IFunctionStore functionStore)
    {
        var orderNumber = Random.Shared.Next(1000, 9999).ToString();
        var json = 
            @"{""OrderNumber"":""OLD"",""ProductIds"":[""5c79603b-5d66-49d5-a46d-cc8a8cbeaf2a"",""bee5774e-9ad7-4668-8e9c-17bc72ee383b""]}"
                .Replace("OLD", orderNumber);
        
        Log.Logger.Information("Start processing of new order with order number {orderId}", orderNumber);
        var functionId = new FunctionId(nameof(OrderProcessor), orderNumber);
        await functionStore.CreateFunction(
            functionId,
            new StoredParameter(json, ParamType: typeof(OrderProcessor.Order).SimpleQualifiedName()),
            storedScrapbook: new StoredScrapbook(
                JsonSerializer.Serialize(new OrderProcessor.Scrapbook()),
                typeof(OrderProcessor.Scrapbook).SimpleQualifiedName()
            ),
            crashedCheckFrequency: 1000,
            version: 0
        );
    }
}