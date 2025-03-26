using DotLox.Extensions;

namespace DotLox;

public class LoxInstance
{
    private LoxClass _class;
    private readonly Dictionary<string, object?> _fields = new();

    public LoxInstance(LoxClass @class)
    {
        _class = @class;
    }

    public object? Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        var method = _class.FindMethod(name.Lexeme);
        if (method != null) return method.Bind(this);

        throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
    }

    public void Set(Token name, object? value)
    {
        _fields.Put(name.Lexeme, value);
    }
    
    public override string ToString()
    {
        return _class.Name + " instance";
    } 
    

}