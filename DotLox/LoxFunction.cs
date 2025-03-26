namespace DotLox;

public class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function _declaration;
    private readonly LoxEnvironment _closure;

    private readonly bool _isInitializer;

    public LoxFunction(Stmt.Function declaration, LoxEnvironment closure, bool isInitializer)
    {
        _closure = closure;
        _isInitializer = isInitializer;
        _declaration = declaration;
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new LoxEnvironment(_closure);
        environment.Define("this", instance);
        return new LoxFunction(_declaration, environment, _isInitializer);
    }
    
    public int Arity()
    {
        return _declaration.Params.Count;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new LoxEnvironment(_closure);
        for (var i = 0; i < _declaration.Params.Count; i++)
        {
            environment.Define(_declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.Body, environment);
        }
        catch (Return returnValue)
        {
            if (_isInitializer) return _closure.GetAt(0, "this");
            
            return returnValue.Value;    
        }

        if (_isInitializer) return _closure.GetAt(0, "this");
        return null;
    }

    public override string ToString() => $"<fn {_declaration.Name.Lexeme} >";
}