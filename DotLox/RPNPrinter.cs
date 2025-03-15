namespace DotLox;

public class RPNPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }
    
    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Group(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Group("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        if (expr.Value == null) return "nil";
        return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Group(expr.Operator.Lexeme, expr.Right);
    }

    private string Group(string name, params Expr[] exprs)
    {
        var result = "";
        foreach (var expr in exprs)
        {
            result += expr.Accept(this);
            result += " ";
        }

        result += $"{name}";

        return result;
    }
}