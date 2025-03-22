﻿namespace DotLox;

public abstract class Stmt
{
	public interface IVisitor<T>
	{
		public T VisitBlockStmt(Block stmt);
		public T VisitBreakStmt(Break stmt);
		public T VisitExpressionStmt(Expression stmt);
		public T VisitIfStmt(If stmt);
		public T VisitPrintStmt(Print stmt);
		public T VisitVarStmt(Var stmt);
		public T VisitWhileStmt(While stmt);
	}

	public class Block : Stmt
	{
		public List<Stmt> Statements { get; }

		public Block(List<Stmt> statements)
		{
			Statements = statements;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBlockStmt(this);
		}
	}

	public class Break : Stmt
	{
		public Token Keyword { get; }

		public Break(Token keyword)
		{
			Keyword = keyword;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBreakStmt(this);
		}
	}

	public class Expression : Stmt
	{
		public Expr Expr { get; }

		public Expression(Expr expr)
		{
			Expr = expr;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitExpressionStmt(this);
		}
	}

	public class If : Stmt
	{
		public Expr Condition { get; }
		public Stmt ThenBranch { get; }
		public Stmt? ElseBranch { get; }

		public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
		{
			Condition = condition;
			ThenBranch = thenBranch;
			ElseBranch = elseBranch;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitIfStmt(this);
		}
	}

	public class Print : Stmt
	{
		public Expr Expr { get; }

		public Print(Expr expr)
		{
			Expr = expr;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitPrintStmt(this);
		}
	}

	public class Var : Stmt
	{
		public Token Name { get; }
		public Expr Initializer { get; }

		public Var(Token name, Expr initializer)
		{
			Name = name;
			Initializer = initializer;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitVarStmt(this);
		}
	}

	public class While : Stmt
	{
		public Expr Condition { get; }
		public Stmt Body { get; }

		public While(Expr condition, Stmt body)
		{
			Condition = condition;
			Body = body;
		}

		public override T Accept<T>(IVisitor<T> visitor)
		{
			return visitor.VisitWhileStmt(this);
		}
	}

	public abstract T Accept<T>(IVisitor<T> visitor);
}
