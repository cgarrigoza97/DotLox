using DotLox.Enums;
using DotLox.Extensions;

namespace DotLox;

public class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{   
    public LoxEnvironment Globals { get; }
    private LoxEnvironment _loxEnvironment;
    private readonly Dictionary<Expr, int> _locals = new();

    public Interpreter()
    {
        Globals = new LoxEnvironment();
        _loxEnvironment = Globals;
        
        Globals.Define("clock", new Native.ClockFunction());
    }
    
    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            DotLox.RuntimeError(e);
        }
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        if (_locals.TryGetValue(expr, out var distance))
        {
            _loxEnvironment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            Globals.Assign(expr.Name, value);
        }
        
        return value;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return IsEqual(left, right);
            case TokenType.Greater:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left >= (double)right;
            case TokenType.Less:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left <= (double)right;
            case TokenType.Minus:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left - (double)right;
            case TokenType.Plus:
                if (left is double && right is double) 
                    return (double)left + (double)right;
                
                if (left is string && right is string)
                    return (string)left + (string)right;
                
                throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings");
            case TokenType.Slash:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left / (double)right;
            case TokenType.Star:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
        }

        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);

        var arguments = new List<object>();
        foreach (var argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not ILoxCallable function)
        {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        }

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
        }

        return function.Call(this, arguments);
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        var @object = Evaluate(expr.Object);
        if (@object is LoxInstance instance)
        {
            return instance.Get(expr.Name);
        }

        throw new RuntimeError(expr.Name, "Only instances have properties");
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }
        
        return Evaluate(expr.Right);
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        var @object = Evaluate(expr.Object);

        if (@object is not LoxInstance)
        {
            throw new RuntimeError(expr.Name, "Only instances have fields.");
        }

        var value = Evaluate(expr.Value);
        ((LoxInstance)@object).Set(expr.Name, value);
        return value;
    }

    public object? VisitSuperExpr(Expr.Super expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
        {
            var superclass = (LoxClass)_loxEnvironment.GetAt(distance, "super")!;
            var @object = (LoxInstance)_loxEnvironment.GetAt(distance - 1, "this")!;

            var method = superclass.FindMethod(expr.Method.Lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.Method, $"Undefined property {expr.Method.Lexeme}.");
            }
            
            return method.Bind(@object);
        }

        return null;
    }

    public object? VisitThisExpr(Expr.This expr)
    {
        return LookUpVariable(expr.Keyword, expr);
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.Bang:
                return !IsTruthy(right);
            case TokenType.Minus:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right;
        }

        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return LookUpVariable(expr.Name, expr);
    }

    private object? LookUpVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
        {
            return _loxEnvironment.GetAt(distance, name.Lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    private void CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token @operator, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(@operator, "Operands must be a number.");
    }

    private bool IsTruthy(object @object)
    {
        if (@object == null) return false;
        if (@object is bool) return (bool)@object;
        return true;
    }

    private bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;
        
        return a.Equals(b);
    }

    private string Stringify(object @object)
    {
        if (@object == null) return "nil";

        if (@object is double)
        {
            var text = @object.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }
        
        return @object.ToString();
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public void Resolve(Expr expr, int depth)
    {
        _locals.Put(expr, depth);
    }

    public void ExecuteBlock(List<Stmt> statements, LoxEnvironment environment)
    {
        var previous = _loxEnvironment;
        try
        {
            _loxEnvironment = environment;

            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            _loxEnvironment = previous;
        }
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new LoxEnvironment(_loxEnvironment));
        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        object? superclass = null;
        if (stmt.Superclass != null)
        {
            superclass = Evaluate(stmt.Superclass);
            if (superclass is not LoxClass)
            {
                throw new RuntimeError(stmt.Superclass.Name, "Superclass must be a class.");
            }
        }
        
        _loxEnvironment.Define(stmt.Name.Lexeme, null);

        if (stmt.Superclass != null)
        {
            _loxEnvironment = new LoxEnvironment(_loxEnvironment);
            _loxEnvironment.Define("super", superclass);
        }

        var methods = new Dictionary<string, LoxFunction>();
        foreach (var method in stmt.Methods)
        {
            var function = new LoxFunction(method, _loxEnvironment, method.Name.Lexeme.Equals("init"));
            methods.Put(method.Name.Lexeme, function);
        }
        
        var @class = new LoxClass(stmt.Name.Lexeme, (LoxClass?)superclass, methods);

        if (superclass != null)
        {
            _loxEnvironment = _loxEnvironment.Enclosing!;
        }
        
        _loxEnvironment.Assign(stmt.Name, @class);
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxFunction(stmt, _loxEnvironment, false);
        _loxEnvironment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.Value != null) value = Evaluate(stmt.Value);

        throw new Return(value);
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }
        
        _loxEnvironment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return null;
    }
}