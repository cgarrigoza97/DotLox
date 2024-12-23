namespace DotLox;

public class Native
{
    public class ClockFunction : ILoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        }
    
        public override string ToString() => "<native fn>";
    }
}