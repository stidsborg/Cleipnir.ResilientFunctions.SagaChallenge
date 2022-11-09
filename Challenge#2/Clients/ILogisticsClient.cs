namespace SagaChallenge2.Clients;

public interface ILogisticsClient
{
    Task ShipProducts(string orderId, Guid customerId, Brand brand, IEnumerable<Guid> productIds);
}