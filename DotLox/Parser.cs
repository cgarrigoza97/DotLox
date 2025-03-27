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

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }
    
    private Expr Expression()
    {
        return Assignment();
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.Class)) return ClassDeclaration();
            if (Match(TokenType.Fun)) return Function("function");
            if (Match(TokenType.Var)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError e)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect class name.");
        Consume(TokenType.LeftBrace, "Expect '{' before class body.");

        var methods = new List<Stmt.Function>();
        var staticMethods = new List<Stmt.Function>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            if (Match(TokenType.Class)) staticMethods.Add(Function("static method"));
            else methods.Add(Function("method"));
        }

        Consume(TokenType.RightBrace, "Expect '}' after class body.");

        return new Stmt.Class(name, methods, staticMethods);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.For)) return ForStatement();
        if (Match(TokenType.If)) return IfStatement();
        if (Match(TokenType.Print)) return PrintStatement();
        if (Match(TokenType.Return)) return ReturnStatement();
        if (Match(TokenType.While)) return WhileStatement();
        if (Match(TokenType.LeftBrace)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");
        
        Stmt? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else if (Match(TokenType.Var))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }
        
        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = Expression();
        }
        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
        
        Expr? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = Expression();
        }
        Consume(TokenType.RightParen, "Expect ')' after for clauses.");

        var body = Statement();

        if (increment != null)
        {
            body = new Stmt.Block([body, new Stmt.Expression(increment)]);
        }

        if (condition == null) condition = new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block([initializer, body]);
        }
        
        return body;
    }

    private Stmt IfStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after if condition.");
        
        var thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
        {
            elseBranch = Statement();
        }
        
        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        
        return new Stmt.Print(value);
    }

    private Stmt ReturnStatement()
    {
        var keyword = Previous();
        Expr? value = null;
        if (!Check(TokenType.Semicolon))
        {
            value = Expression();
        }
        
        Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.");
        
        Expr initializer = null;
        if (Match(TokenType.Equal))
        {
            initializer = Expression();
        }
        
        Consume(TokenType.Semicolon, "Expect ';' variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RightParen, "Expect ')' after condition.");
        var body = Statement();
        
        return new Stmt.While(condition, body);
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        
        return new Stmt.Expression(expr);
    }

    private Stmt.Function Function(string kind)
    {
        var name = Consume(TokenType.Identifier, $"Expect ${kind} name.");
        Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");
        var parameters = new List<Token>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expect ')' after parameters.");
        
        Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
        var body = Block();
        return new Stmt.Function(name, parameters, body);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return statements;
    }

    private Expr Assignment()
    {
        var expr = Or();

        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                var name = variable.Name;
                return new Expr.Assign(name, value);
            }
            
            if (expr is Expr.Get get)
            {
                return new Expr.Set(get.Object, get.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(TokenType.Or))
        {
            var @operator = Previous();
            var right = And();
            expr = new Expr.Logical(expr, @operator, right);
        }
        
        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.And))
        {
            var @operator = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
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

        return Call();
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                arguments.Add(Expression());
            } 
            while (Match(TokenType.Comma));
        }

        var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");
        
        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenType.Dot))
            {
                var name = Consume(TokenType.Identifier, "Expect property name after '.'.");
                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }
        
        return expr;
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

        if (Match(TokenType.This)) return new Expr.This(Previous());

        if (Match(TokenType.Identifier))
        {
            return new Expr.Variable(Previous());
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