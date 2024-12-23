namespace DotLox;

public class Return : Exception
{
    public object Value { get; }

    public Return(object value) : base()
    {
        Value = value;
    }
}