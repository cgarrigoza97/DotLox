namespace DotLox;

public class LoxEnvironment
{
    private readonly Dictionary<string, ValueHolder> _values = new();
    
    public LoxEnvironment? Enclosing { get; }

    public LoxEnvironment()
    {
        Enclosing = null;
    }

    public LoxEnvironment(LoxEnvironment enclosing)
    {
        Enclosing = enclosing;
    } 
    
    public void Define(string name, bool initialize, object? value)
    {
        var valueHolder = new ValueHolder();
        if (value is null && initialize)
        {
            valueHolder.SetValue(value);
        }
        
        _values.Add(name, valueHolder);
    }

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
        {
            if (!value.Initialized) throw new RuntimeError(name, $"Not assigned variable {name.Lexeme}");
            
            return value.Value;
        }
        
        if (Enclosing != null) return Enclosing.Get(name);
        
        throw new RuntimeError(name, $"Undefined variable {name.Lexeme}.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = new ValueHolder(value);
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable {name.Lexeme}.");
    }

    private class ValueHolder
    {
        public bool Initialized { get; set; }
        public object? Value { get; set;  }

        public ValueHolder()
        {
            Initialized = false;
            Value = null;
        }

        public ValueHolder(object? value)
        {
            Initialized = true;
            Value = value;
        }

        public void SetValue(object? value)
        {
            Initialized = true;
            Value = value;
        }
    }
}
