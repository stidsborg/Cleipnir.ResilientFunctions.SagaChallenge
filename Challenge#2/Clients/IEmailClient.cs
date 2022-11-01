namespace SagaChallenge2.Clients;

public interface IEmailClient
{
    Task SendOrderConfirmation(Guid customerId, Brand brand, IEnumerable<Guid> productIds);
}