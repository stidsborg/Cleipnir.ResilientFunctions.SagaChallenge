namespace SagaChallenge3;

public class MessageBroker
{
    private readonly Action<object, MessageBroker> _callback;

    private readonly List<object> _messages = new();

    public IReadOnlyList<object> Messages
    {
        get
        {
            lock (_messages)
                return _messages.ToList();
        }
    }

    public MessageBroker(Action<object, MessageBroker> callback) => _callback = callback;

    public void Send(object msg)
    {
        lock (_messages)
            _messages.Add(msg);
        
        _callback(msg, this);  
    } 
}