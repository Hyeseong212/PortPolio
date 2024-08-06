public class CompositeKey : IEquatable<CompositeKey>
{
    public string Type { get; }
    public int MessageCode { get; }

    public CompositeKey(string type, int messageCode)
    {
        Type = type;
        MessageCode = messageCode;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as CompositeKey);
    }

    public bool Equals(CompositeKey other)
    {
        return other != null &&
               Type == other.Type &&
               MessageCode == other.MessageCode;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, MessageCode);
    }
}