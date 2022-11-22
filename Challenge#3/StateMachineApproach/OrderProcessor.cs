namespace SagaChallenge3.StateMachineApproach;

public class OrderProcessor :
    IHandleMessage<FundsReserved>, 
    IHandleMessage<ProductsShipped>, 
    IHandleMessage<FundsCaptured>, 
    IHandleMessage<OrderConfirmationEmailSent>
{
    private MessageBroker MessageBroker { get; }
    private Order Order { get; }
    public bool Completed { get; set; }

    public OrderProcessor(MessageBroker messageBroker, Order order)
    {
        MessageBroker = messageBroker;
        Order = order;
    }

    public void Handle(FundsReserved fundsReserved)
    {

    }
    
    public void Handle(ProductsShipped productsShipped)
    {

    }
    
    public void Handle(FundsCaptured fundsCaptured)
    {

    }
    
    public void Handle(OrderConfirmationEmailSent orderConfirmationEmailSent)
    {
        
    }

    public void ExecuteNextStep()
    {
        
    }
}