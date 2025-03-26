using DotLox.Enums;

namespace DotLox;

public class DotLox
{
    private static readonly Interpreter _interpreter = new Interpreter(); 
    private static bool hadError = false;
    private static bool hadRuntimeError = false;
    
    public static void Init(string[] args)
    {
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: DotLox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    private static void RunFile(string path)
    {
        var fileAsString = File.ReadAllText(path);
        Run(fileAsString);
        
        if (hadError) Environment.Exit(65);
        if (hadRuntimeError) Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            hadError = false;
        }
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens);
        var statements = parser.Parse();

        if (hadError) return;

        var resolver = new Resolver(_interpreter);
        resolver.Resolve(statements);

        if (hadError) return;
        
        _interpreter.Interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.Error.WriteLine($"{error.Message} \n[line {error.Token.Line}]");
        hadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.Eof)
        {
            Report(token.Line, "at end", message);
        }
        else
        {
            Report(token.Line, $"at {token.Lexeme}", message);
        }
    }
    
}