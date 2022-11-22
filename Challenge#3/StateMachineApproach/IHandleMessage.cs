namespace SagaChallenge3.StateMachineApproach;

public interface IHandleMessage<in T>
{
    void Handle(T msg);
}