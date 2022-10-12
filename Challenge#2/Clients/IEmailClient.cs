using Serilog;

namespace SagaChallenge2.Clients;

public interface IEmailClient
{
    Task SendOrderConfirmation(Guid customerId, Brand brand, IEnumerable<Guid> productIds);
}

public class EmailClientStub : IEmailClient
{
    public Task SendOrderConfirmation(Guid customerId, Brand brand, IEnumerable<Guid> productIds)
        => Task.Delay(100).ContinueWith(_ => 
            Log.Logger.ForContext<IEmailClient>().Information("EMAIL_SERVER: Order confirmation emailed for brand: {Brand}", brand)
        );
}