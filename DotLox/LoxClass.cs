namespace DotLox;

public class LoxClass : ILoxCallable
{
    public string Name { get; }
    private readonly Dictionary<string, LoxFunction> _methods;
    private readonly Dictionary<string, LoxFunction> _staticMethods;
    private readonly Dictionary<string, LoxFunction> _getters;
    
    public LoxClass(string name, Dictionary<string, LoxFunction> methods, Dictionary<string, LoxFunction> staticMethods, Dictionary<string, LoxFunction> getters)
    {
        Name = name;
        _methods = methods;
        _staticMethods = staticMethods;
        _getters = getters;
    }

    public LoxFunction? FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }

    public LoxFunction? FindStaticMethod(string name)
    {
        if (_staticMethods.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }

    public LoxFunction? FindGetter(string name)
    {
        if (_getters.TryGetValue(name, out var value))
        {
            return value;
        }

        return null;
    }

    public override string ToString()
    {
        return Name;
    }

    public int Arity()
    {
        var initializer = FindMethod("init");
        if (initializer == null) return 0;
        return initializer.Arity();
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new LoxInstance(this);
        var initializer = FindMethod("init");
        if (initializer != null)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }
        
        return instance;
    }
}