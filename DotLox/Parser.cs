using DotLox.Enums;

namespace DotLox;

public class Parser
{
    private class ParseError : Exception {}
    
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public Expr Parse()
    {
        try
        {
            return Expression();
        }
        catch (ParseError error)
        {
            return null;
        }
    }
    
    private Expr Expression()
    {
        return Comma();
    }

    private Expr Comma()
    {
        return HandleLeftAssociativeBinaryOperator(Equality, TokenType.Comma);
    }

    private Expr Equality()
    {
        return HandleLeftAssociativeBinaryOperator(Comparison, TokenType.BangEqual, TokenType.EqualEqual);
    }
    
    
    private Expr Comparison()
    {
        return HandleLeftAssociativeBinaryOperator(Term, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual);
    }

    private Expr Term()
    {
        return HandleLeftAssociativeBinaryOperator(Factor, TokenType.Minus, TokenType.Plus);
    }

    private Expr Factor()
    {
        return HandleLeftAssociativeBinaryOperator(Unary, TokenType.Slash, TokenType.Star);
    }

    private Expr Unary()
    {
        if (Match(TokenType.Bang, TokenType.Less))
        {
            var @operator = Previous();
            var right = Unary();
            return new Expr.Unary(@operator, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.False)) return new Expr.Literal(false);
        if (Match(TokenType.True)) return new Expr.Literal(true);
        if (Match(TokenType.Nil)) return new Expr.Literal(null);

        if (Match(TokenType.Number, TokenType.String))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(TokenType.LeftParen))
        {
            var expr = Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        
        throw Error(Peek(), "Expect expression.");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.Eof;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private ParseError Error(Token token, string message)
    {
        DotLox.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                        return;
            }

            Advance();
        }
    } 
    
    private Expr HandleLeftAssociativeBinaryOperator(Func<Expr> operand, params TokenType[] types)
    {
        var expr = operand();

        while (Match(types))
        {
            var @operator = Previous();
            var right = operand();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }
}