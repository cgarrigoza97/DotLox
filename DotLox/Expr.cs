namespace DotLox;

public abstract class Expr
{
	public interface Visitor<T>
	{
		public T VisitBinaryExpr(Binary expr);
		public T VisitTernaryExpr(Ternary expr);
		public T VisitGroupingExpr(Grouping expr);
		public T VisitLiteralExpr(Literal expr);
		public T VisitUnaryExpr(Unary expr);
	}

	public class Binary : Expr
	{
		public Expr Left { get; }
		public Token Operator { get; }
		public Expr Right { get; }

		public Binary(Expr left, Token @operator, Expr right)
		{
			Left = left;
			Operator = @operator;
			Right = right;
		}

		public override T Accept<T>(Visitor<T> visitor)
		{
			return visitor.VisitBinaryExpr(this);
		}
	}

	public class Ternary : Expr
	{
		public Expr First { get; }
		public Expr Second { get; }
		public Expr Third { get; }

		public Ternary(Expr first, Expr second, Expr third)
		{
			First = first;
			Second = second;
			Third = third;
		}

		public override T Accept<T>(Visitor<T> visitor)
		{
			return visitor.VisitTernaryExpr(this);
		}
	}

	public class Grouping : Expr
	{
		public Expr Expression { get; }

		public Grouping(Expr expression)
		{
			Expression = expression;
		}

		public override T Accept<T>(Visitor<T> visitor)
		{
			return visitor.VisitGroupingExpr(this);
		}
	}

	public class Literal : Expr
	{
		public object Value { get; }

		public Literal(object value)
		{
			Value = value;
		}

		public override T Accept<T>(Visitor<T> visitor)
		{
			return visitor.VisitLiteralExpr(this);
		}
	}

	public class Unary : Expr
	{
		public Token Operator { get; }
		public Expr Right { get; }

		public Unary(Token @operator, Expr right)
		{
			Operator = @operator;
			Right = right;
		}

		public override T Accept<T>(Visitor<T> visitor)
		{
			return visitor.VisitUnaryExpr(this);
		}
	}

	public abstract T Accept<T>(Visitor<T> visitor);
}
