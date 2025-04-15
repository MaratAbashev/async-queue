namespace Domain.Models;

public class Message
{
    public string KeyJson { get; set; }
    public string KeyType { get; set; }
    public string ValueJson { get; set; }
    public string ValueType { get; set; }
}

public class Message<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
}