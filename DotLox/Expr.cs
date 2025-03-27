namespace DotLox;

public abstract class Expr
{
	public interface IVisitor<T>
	{
		public T VisitAssignExpr(Assign expr);
		public T VisitBinaryExpr(Binary expr);
		public T VisitCallExpr(Call expr);
		public T VisitGetExpr(Get expr);
		public T VisitGroupingExpr(Grouping expr);
		public T VisitLiteralExpr(Literal expr);
		public T VisitLogicalExpr(Logical expr);
		public T VisitSetExpr(Set expr);
		public T VisitSuperExpr(Super expr);
		public T VisitThisExpr(This expr);
		public T VisitUnaryExpr(Unary expr);
		public T VisitVariableExpr(Variable expr);
	}

	public class Assign : Expr
	{
		public Token Name { get; }
		public Expr Value { get; }

		public Assign(Token name, Expr value)
		{
			Name = name;
			Value = value;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitAssignExpr(this);
		}
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

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBinaryExpr(this);
		}
	}

	public class Call : Expr
	{
		public Expr Callee { get; }
		public Token Paren { get; }
		public List<Expr> Arguments { get; }

		public Call(Expr callee, Token paren, List<Expr> arguments)
		{
			Callee = callee;
			Paren = paren;
			Arguments = arguments;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitCallExpr(this);
		}
	}

	public class Get : Expr
	{
		public Expr Object { get; }
		public Token Name { get; }

		public Get(Expr @object, Token name)
		{
			Object = @object;
			Name = name;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitGetExpr(this);
		}
	}

	public class Grouping : Expr
	{
		public Expr Expression { get; }

		public Grouping(Expr expression)
		{
			Expression = expression;
		}

		public override T Accept<T>(IVisitor<T> visitor)
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

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitLiteralExpr(this);
		}
	}

	public class Logical : Expr
	{
		public Expr Left { get; }
		public Token Operator { get; }
		public Expr Right { get; }

		public Logical(Expr left, Token @operator, Expr right)
		{
			Left = left;
			Operator = @operator;
			Right = right;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitLogicalExpr(this);
		}
	}

	public class Set : Expr
	{
		public Expr Object { get; }
		public Token Name { get; }
		public Expr Value { get; }

		public Set(Expr @object, Token name, Expr value)
		{
			Object = @object;
			Name = name;
			Value = value;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitSetExpr(this);
		}
	}

	public class Super : Expr
	{
		public Token Keyword { get; }
		public Token Method { get; }

		public Super(Token keyword, Token method)
		{
			Keyword = keyword;
			Method = method;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitSuperExpr(this);
		}
	}

	public class This : Expr
	{
		public Token Keyword { get; }

		public This(Token keyword)
		{
			Keyword = keyword;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitThisExpr(this);
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

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitUnaryExpr(this);
		}
	}

	public class Variable : Expr
	{
		public Token Name { get; }

		public Variable(Token name)
		{
			Name = name;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitVariableExpr(this);
		}
	}

	public abstract T Accept<T>(IVisitor<T> visitor);
}
