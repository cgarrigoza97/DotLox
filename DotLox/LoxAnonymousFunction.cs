namespace DotLox;

public class LoxAnonymousFunction : ILoxCallable
{
    private readonly Expr.Function _definition;
    private readonly LoxEnvironment _closure;

    public LoxAnonymousFunction(Expr.Function definition, LoxEnvironment closure)
    {
        _closure = closure;
        _definition = definition;
    }
    
    public int Arity()
    {
        return _definition.Params.Count;
    }

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new LoxEnvironment(_closure);
        for (var i = 0; i < _definition.Params.Count; i++)
        {
            environment.Define(_definition.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_definition.Body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;    
        }
        
        return null;
    }

    public override string ToString() => $"<fn Anonymous >";
}