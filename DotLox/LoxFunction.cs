namespace DotLox;

public class LoxFunction : ILoxCallable
{
    private readonly Stmt.Function _declaration;
    private readonly LoxEnvironment _closure;

    public LoxFunction(Stmt.Function declaration, LoxEnvironment closure)
    {
        _closure = closure;
        _declaration = declaration;
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
            return returnValue.Value;    
        }
        
        return null;
    }

    public override string ToString() => $"<fn {_declaration.Name.Lexeme} >";
}