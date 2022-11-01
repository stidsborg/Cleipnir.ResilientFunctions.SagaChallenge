namespace SagaChallenge2.Clients;

public interface ILogisticsClient
{
    Task ShipProducts(Guid customerId, Brand brand, IEnumerable<Guid> productIds);
}