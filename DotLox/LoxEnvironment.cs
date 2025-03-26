using DotLox.Extensions;

namespace DotLox;

public class LoxEnvironment
{
    private readonly Dictionary<string, object?> _values = new();
    
    public LoxEnvironment? Enclosing { get; }

    public LoxEnvironment()
    {
        Enclosing = null;
    }

    public LoxEnvironment(LoxEnvironment enclosing)
    {
        Enclosing = enclosing;
    } 
    
    public void Define(string name, object? value)
    {
        _values.Put(name, value);
    }

    public object? GetAt(int distance, string name)
    {
        Ancestor(distance)._values.TryGetValue(name, out var value);
        return value;
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)._values.Put(name.Lexeme, value);
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }
        
        if (Enclosing != null) return Enclosing.Get(name);
        
        throw new RuntimeError(name, $"Undefined variable {name.Lexeme}.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values.Put(name.Lexeme, value);
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable {name.Lexeme}.");
    }

    private LoxEnvironment Ancestor(int distance)
    {
        var loxEnvironment = this;
        for (var i = 0; i < distance; i++)
        {
            loxEnvironment = loxEnvironment!.Enclosing;
        }

        return loxEnvironment!;
    }
}
