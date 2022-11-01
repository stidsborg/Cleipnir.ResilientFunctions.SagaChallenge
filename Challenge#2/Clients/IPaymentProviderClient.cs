namespace SagaChallenge2.Clients;

public interface IPaymentProviderClient
{
    Task Reserve(Guid transactionId, decimal amount);
    Task Capture(Guid transactionId);
    Task CancelReservation(Guid transactionId);
}