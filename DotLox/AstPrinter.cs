namespace DotLox;

public class AstPrinter : Expr.IVisitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitCallExpr(Expr.Call expr)
    {
        throw new NotImplementedException();
    }

    public string VisitGetExpr(Expr.Get expr)
    {
        throw new NotImplementedException();
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        if (expr.Value == null) return "nil";
        return expr.Value.ToString();
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }

    public string VisitSetExpr(Expr.Set expr)
    {
        throw new NotImplementedException();
    }

    public string VisitSuperExpr(Expr.Super expr)
    {
        throw new NotImplementedException();
    }

    public string VisitThisExpr(Expr.This expr)
    {
        throw new NotImplementedException();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
        return Parenthesize(expr.Name.Lexeme);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var result = $"({name}";
        foreach (var expr in exprs)
        {
            result += " ";
            result += expr.Accept(this);
        }

        result += ")";

        return result;
    }
}