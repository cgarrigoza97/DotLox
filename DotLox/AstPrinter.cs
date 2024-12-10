namespace DotLox;

public class AstPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }
    
    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
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

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
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