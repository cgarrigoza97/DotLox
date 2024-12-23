namespace DotLox;

public interface ILoxCallable
{
    public int Arity();
    public object? Call(Interpreter interpreter, List<object> arguments);
}