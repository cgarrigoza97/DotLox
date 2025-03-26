using DotLox.Enums;
using DotLox.Extensions;

namespace DotLox;

public class Resolver : Stmt.IVisitor<object?>, Expr.IVisitor<object?>
{
    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, VariableInformation>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None; 
    
    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();

        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.Function);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (_currentFunction == FunctionType.None)
        {
            DotLox.Error(stmt.Keyword, "Can't return from top-level code.");
        }
        
        if (stmt.Value != null)
        {
            Resolve(stmt.Value);
        }

        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);

        return null;
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (var argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out var value) && value.Defined == false)
        {
            DotLox.Error(expr.Name, "Can't read local variable in its own initializer");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, VariableInformation>());
    }

    private void EndScope()
    {
        CheckUnusedVariables();
        _scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;

        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            DotLox.Error(name, "Already a variable with this name in this scope.");
        }
        
        scope.Put(name.Lexeme, new VariableInformation(name));
        
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;

        _scopes.Peek().TryGetValue(name.Lexeme, out var value);

        value ??= new VariableInformation(name);

        value.Defined = true;
        
        _scopes.Peek().Put(name.Lexeme, value);
    }

    private void MarkAsUsed(Token name)
    {
        if (_scopes.Count == 0) return;

        _scopes.Peek().TryGetValue(name.Lexeme, out var value);

        value!.Used = true;
        
        _scopes.Peek().Put(name.Lexeme, value);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var scopes = _scopes.ToArray();
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            if (scopes[i].ContainsKey(name.Lexeme))
            {
                MarkAsUsed(name);
                _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;
        
        BeginScope();
        foreach (var @param in function.Params)
        {
            Declare(param);
            Define(param);
        }
        
        Resolve(function.Body);
        EndScope();

        _currentFunction = enclosingFunction;
    }

    private void CheckUnusedVariables()
    {
        if (_scopes.Count != 0)
        {
            foreach (var variable in _scopes.Peek())
            {
                if (!variable.Value.Used) DotLox.Error(variable.Value.Name, $"Unused variable '{variable.Value.Name.Lexeme}'");
            }
        } 
    }

    private class VariableInformation
    {
        public bool Defined { get; set; }
        public bool Used { get; set; }
        public Token Name { get; }
        
        public VariableInformation(Token name)
        {
            Name = name;
        }
    }
}