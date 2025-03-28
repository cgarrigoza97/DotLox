﻿namespace DotLox;

public class LoxClass : ILoxCallable
{
    public string Name { get; }
    public LoxClass? Superclass { get; }
    private readonly Dictionary<string, LoxFunction> _methods;
    
    public LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        _methods = methods;
        Superclass = superclass;
    }

    public LoxFunction? FindMethod(string name)
    {
        if (_methods.TryGetValue(name, out var value))
        {
            return value;
        }

        if (Superclass != null)
        {
            return Superclass.FindMethod(name);
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