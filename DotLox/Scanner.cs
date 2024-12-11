using DotLox.Enums;

namespace DotLox;

public class Scanner
{
    private static readonly Dictionary<string, TokenType> _keywords;
    
    private readonly string _source;
    private readonly List<Token> _tokens = [];

    private int start = 0;
    private int current = 0;
    private int line = 1;
    
    public Scanner(string source)
    {
        _source = source;
    }
    
    static Scanner()
    {
        _keywords = new Dictionary<string, TokenType>
        {
            { "and", TokenType.And },
            { "class", TokenType.Class },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "fun", TokenType.Fun },
            { "if", TokenType.If },
            { "nil", TokenType.Nil },
            { "or", TokenType.Or },
            { "print", TokenType.Print },
            { "return", TokenType.Return },
            { "super", TokenType.Super },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "var", TokenType.Var },
            { "while", TokenType.While }
        };
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }
        
        _tokens.Add(new Token(TokenType.Eof, "", null, line));
        return _tokens;
    }

    private bool IsAtEnd()
    {
        return current >= _source.Length;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break; 
            case '!':
                AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            case '/':
                if (Match('/'))
                {
                    // Comment is consumed until the end of the line
                    while (Peek() != '\n' && !IsAtEnd()) Advance();
                }
                else if (Match('*'))
                {
                    BlockCommentHandler();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespaces.
                break;

            case '\n':
                line++;
                break;
            case '"': StringHandler(); break;
            default:
                if (IsDigit(c))
                {
                    NumberHandler();
                }
                else if (IsAlpha(c))
                {
                    Identifier();
                }
                else
                {
                    DotLox.Error(line, $"Unexpected character");
                }
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        
        var text = _source.Substring(start, current - start);
        var type = _keywords.ContainsKey(text) ? _keywords[text] : TokenType.Identifier;
        
        AddToken(type);
    }

    private void NumberHandler()
    {
        while (IsDigit(Peek())) Advance();

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            
            while (IsDigit(Peek())) Advance();
        }
        
        AddToken(TokenType.Number, double.Parse(_source.Substring(start, current - start)));
    }

    private void StringHandler()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd())
        {
            DotLox.Error(line, "Unterminated string.");
            return;
        }
        
        // The closing '"'.
        Advance();
        
        var value = _source.Substring(start, current - start);
        AddToken(TokenType.String, value);
    }

    private void BlockCommentHandler()
    {
        // Peek() != '\n' && !IsAtEnd()
        var peekedChar = Peek();
        if (peekedChar != '*')
        {
            if (peekedChar == '\n') line++;
            Advance();
        }

        while (peekedChar != '*' && !IsAtEnd())
        {
            
        }
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[current] != expected) return false;

        current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[current];
    }

    private char PeekNext()
    {
        if (current + 1 >= _source.Length) return '\0';
        return _source[current + 1];
    }
    
    private bool IsAlpha(char c) {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private bool IsAlphaNumeric(char c) {
        return IsAlpha(c) || IsDigit(c);
    }

    private bool IsDigit(char c)
    {
        return c is >= '0' and <= '9';
    }

    private char Advance()
    {
        return _source[current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object literal)
    {
        var text = _source.Substring(start, current - start);
        _tokens.Add(new Token(type, text, literal, line));
    }
}