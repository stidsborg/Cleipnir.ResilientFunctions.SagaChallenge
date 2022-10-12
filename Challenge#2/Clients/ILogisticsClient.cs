using Serilog;

namespace SagaChallenge2.Clients;

public interface ILogisticsClient
{
    Task ShipProducts(Guid customerId, Brand brand, IEnumerable<Guid> productIds);
}

public class LogisticsClientStub : ILogisticsClient
{
    public Task ShipProducts(Guid customerId, Brand brand, IEnumerable<Guid> productIds)
        => Task.Delay(100).ContinueWith(_ =>
            Log.Logger.ForContext<ILogisticsClient>().Information("LOGISTICS_SERVER: Products shipped for brand: {Brand}", brand)
        );
}