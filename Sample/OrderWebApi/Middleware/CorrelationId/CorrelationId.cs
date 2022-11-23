namespace OrderWebApi.Middleware.CorrelationId;

public class CorrelationId
{
    private readonly AsyncLocal<string> _correlationId = new();

    public string Value
    {
        get => _correlationId.Value!;
        set => _correlationId.Value = value;
    }
}