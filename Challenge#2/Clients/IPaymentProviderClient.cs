using Serilog;

namespace SagaChallenge2.Clients;

public interface IPaymentProviderClient
{
    Task Reserve(Guid transactionId, decimal amount);
    Task Capture(Guid transactionId);
    Task CancelReservation(Guid transactionId);
}

public class PaymentProviderClientStub : IPaymentProviderClient
{
    public Task Reserve(Guid transactionId, decimal amount)
        => Task
            .Delay(100)
            .ContinueWith(_ => Log.Logger.ForContext<IPaymentProviderClient>().Information("PAYMENT_PROVIDER: Reserved '{Amount}' with transaction id: '{TransactionId}'", amount, transactionId))
            .ContinueWith(_ => Guid.NewGuid());
    public Task Capture(Guid transactionId) 
        => Task.Delay(100).ContinueWith(_ => 
            Log.Logger.ForContext<IPaymentProviderClient>().Information("PAYMENT_PROVIDER: Reserved amount captured with transaction id: '{TransactionId}'", transactionId)
        );
    public Task CancelReservation(Guid transactionId) 
        => Task.Delay(100).ContinueWith(_ => 
            Log.Logger.ForContext<IPaymentProviderClient>().Information("PAYMENT_PROVIDER: Reservation cancelled  with transaction id: '{TransactionId}'", transactionId)
        );
}