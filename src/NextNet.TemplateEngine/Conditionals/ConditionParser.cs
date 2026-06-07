using System.Globalization;
using NextNet.TemplateEngine.Conditionals.Ast;

namespace NextNet.TemplateEngine.Conditionals;

/// <summary>
/// Recursive descent parser that converts conditional expression strings into an AST.
/// </summary>
/// <remarks>
/// <para>
/// Supported grammar (precedence from lowest to highest):
/// <code>
/// expression     → or_expr
/// or_expr        → and_expr ("||" and_expr)*
/// and_expr       → equality_expr ("&amp;&amp;" equality_expr)*
/// equality_expr  → comparison_expr (("==" | "!=" | "in") comparison_expr)*
/// comparison_expr → add_expr (("&gt;" | "&gt;=" | "&lt;" | "&lt;=") add_expr)?
/// add_expr       → unary_expr (("+") unary_expr)?
/// unary_expr     → "!" unary_expr | primary_expr
/// primary_expr   → "(" expression ")" | variable | literal
/// literal        → STRING | BOOL | NUMBER | "null"
/// variable       → IDENTIFIER ("." IDENTIFIER)*
/// </code>
/// </para>
/// <para>
/// String literals support both single quotes (<c>'text'</c>) and double quotes (<c>"text"</c>).
/// Boolean literals are <c>true</c> and <c>false</c>. The <c>null</c> literal represents
/// a null value.
/// </para>
/// <example>
/// <code>
/// var parser = new ConditionParser();
/// var expr = parser.Parse("features.auth == true &amp;&amp; features.logging");
/// </code>
/// </example>
/// </remarks>
public sealed class ConditionParser
{
    /// <summary>
    /// Parses a conditional expression string into an AST.
    /// </summary>
    /// <param name="expression">The expression string to parse. Must not be null or whitespace.</param>
    /// <returns>The root <see cref="Expression"/> node of the parsed AST.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> is null or empty.</exception>
    /// <exception cref="ParseException">Thrown when the expression contains a syntax error.</exception>
    public Expression Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null or empty.", nameof(expression));

        var tokens = Tokenize(expression);
        var parser = new Parser(tokens, expression);
        var result = parser.ParseExpression();
        if (parser.CurrentToken.Type != TokenType.EndOfInput)
            throw new ParseException($"Unexpected token '{parser.CurrentToken.Value}'", parser.CurrentToken.Position, expression);
        return result;
    }

    /// <summary>
    /// Tokenizes the input string into a list of tokens.
    /// </summary>
    private static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < input.Length)
        {
            // Skip whitespace
            if (char.IsWhiteSpace(input[i]))
            {
                i++;
                continue;
            }

            // Single-line comment
            if (i < input.Length - 1 && input[i] == '/' && input[i + 1] == '/')
            {
                while (i < input.Length && input[i] != '\n')
                    i++;
                continue;
            }

            var ch = input[i];

            // String literals (single or double quotes)
            if (ch == '\'' || ch == '"')
            {
                var quote = ch;
                var start = i;
                i++; // skip opening quote
                while (i < input.Length && input[i] != quote)
                {
                    i++;
                }
                if (i >= input.Length)
                    throw new ParseException("Unterminated string literal", start, input);
                i++; // skip closing quote
                var value = input[(start + 1)..(i - 1)];
                tokens.Add(new Token(TokenType.String, value, start));
                continue;
            }

            // Identifiers and keywords
            if (char.IsLetter(ch) || ch == '_')
            {
                var start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_'))
                    i++;
                var word = input[start..i];
                switch (word)
                {
                    case "true":
                    case "false":
                        tokens.Add(new Token(TokenType.Bool, word, start));
                        break;
                    case "null":
                        tokens.Add(new Token(TokenType.Null, word, start));
                        break;
                    case "in":
                        tokens.Add(new Token(TokenType.Op, "in", start));
                        break;
                    default:
                        tokens.Add(new Token(TokenType.Identifier, word, start));
                        break;
                }
                continue;
            }

            // Number literals
            if (char.IsDigit(ch))
            {
                var start = i;
                while (i < input.Length && char.IsDigit(input[i]))
                    i++;
                if (i < input.Length && input[i] == '.')
                {
                    i++; // consume dot
                    while (i < input.Length && char.IsDigit(input[i]))
                        i++;
                    tokens.Add(new Token(TokenType.Number, input[start..i], start));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Number, input[start..i], start));
                }
                continue;
            }

            // Multi-character operators
            if (i < input.Length - 1)
            {
                var twoChar = input.Substring(i, 2);
                switch (twoChar)
                {
                    case "==":
                        tokens.Add(new Token(TokenType.Op, "==", i));
                        i += 2;
                        continue;
                    case "!=":
                        tokens.Add(new Token(TokenType.Op, "!=", i));
                        i += 2;
                        continue;
                    case "&&":
                        tokens.Add(new Token(TokenType.Op, "&&", i));
                        i += 2;
                        continue;
                    case "||":
                        tokens.Add(new Token(TokenType.Op, "||", i));
                        i += 2;
                        continue;
                    case ">=":
                        tokens.Add(new Token(TokenType.Op, ">=", i));
                        i += 2;
                        continue;
                    case "<=":
                        tokens.Add(new Token(TokenType.Op, "<=", i));
                        i += 2;
                        continue;
                }
            }

            // Single-character operators and punctuation
            switch (ch)
            {
                case '>':
                    tokens.Add(new Token(TokenType.Op, ">", i));
                    i++;
                    continue;
                case '<':
                    tokens.Add(new Token(TokenType.Op, "<", i));
                    i++;
                    continue;
                case '!':
                    tokens.Add(new Token(TokenType.Op, "!", i));
                    i++;
                    continue;
                case '+':
                    tokens.Add(new Token(TokenType.Op, "+", i));
                    i++;
                    continue;
                case '.':
                    tokens.Add(new Token(TokenType.Op, ".", i));
                    i++;
                    continue;
                case '(':
                    tokens.Add(new Token(TokenType.LParen, "(", i));
                    i++;
                    continue;
                case ')':
                    tokens.Add(new Token(TokenType.RParen, ")", i));
                    i++;
                    continue;
                default:
                    throw new ParseException($"Unexpected character '{ch}'", i, input);
            }
        }

        tokens.Add(new Token(TokenType.EndOfInput, "", input.Length));
        return tokens;
    }

    /// <summary>
    /// Internal recursive descent parser that operates on the token list.
    /// </summary>
    private sealed class Parser
    {
        private readonly List<Token> _tokens;
        private readonly string _expression;
        private int _pos;

        public Token CurrentToken => _tokens[_pos];

        public Parser(List<Token> tokens, string expression)
        {
            _tokens = tokens;
            _expression = expression;
            _pos = 0;
        }

        /// <summary>
        /// expression → or_expr
        /// </summary>
        public Expression ParseExpression() => ParseOr();

        /// <summary>
        /// or_expr → and_expr ("||" and_expr)*
        /// </summary>
        private Expression ParseOr()
        {
            var left = ParseAnd();
            while (CurrentToken.Type == TokenType.Op && CurrentToken.Value == "||")
            {
                var op = Consume().Value;
                var right = ParseAnd();
                left = new BinaryExpression(left, op, right);
            }
            return left;
        }

        /// <summary>
        /// and_expr → equality_expr ("&amp;&amp;" equality_expr)*
        /// </summary>
        private Expression ParseAnd()
        {
            var left = ParseEquality();
            while (CurrentToken.Type == TokenType.Op && CurrentToken.Value == "&&")
            {
                var op = Consume().Value;
                var right = ParseEquality();
                left = new BinaryExpression(left, op, right);
            }
            return left;
        }

        /// <summary>
        /// equality_expr → comparison_expr (("==" | "!=" | "in") comparison_expr)*
        /// </summary>
        private Expression ParseEquality()
        {
            var left = ParseComparison();
            while (CurrentToken.Type == TokenType.Op &&
                   (CurrentToken.Value == "==" || CurrentToken.Value == "!=" || CurrentToken.Value == "in"))
            {
                var op = Consume().Value;
                var right = ParseComparison();
                left = new BinaryExpression(left, op, right);
            }
            return left;
        }

        /// <summary>
        /// comparison_expr → add_expr (("&gt;" | "&gt;=" | "&lt;" | "&lt;=") add_expr)?
        /// </summary>
        private Expression ParseComparison()
        {
            var left = ParseAdd();
            if (CurrentToken.Type == TokenType.Op &&
                (CurrentToken.Value == ">" || CurrentToken.Value == ">=" ||
                 CurrentToken.Value == "<" || CurrentToken.Value == "<="))
            {
                var op = Consume().Value;
                var right = ParseAdd();
                left = new BinaryExpression(left, op, right);
            }
            return left;
        }

        /// <summary>
        /// add_expr → unary_expr (("+") unary_expr)*
        /// </summary>
        private Expression ParseAdd()
        {
            var left = ParseUnary();
            while (CurrentToken.Type == TokenType.Op && CurrentToken.Value == "+")
            {
                var op = Consume().Value;
                var right = ParseUnary();
                left = new BinaryExpression(left, op, right);
            }
            return left;
        }

        /// <summary>
        /// unary_expr → "!" unary_expr | primary_expr
        /// </summary>
        private Expression ParseUnary()
        {
            if (CurrentToken.Type == TokenType.Op && CurrentToken.Value == "!")
            {
                var pos = CurrentToken.Position;
                Consume();
                var operand = ParseUnary();
                return new UnaryExpression("!", operand);
            }
            return ParsePrimary();
        }

        /// <summary>
        /// primary_expr → "(" expression ")" | variable | literal
        /// </summary>
        private Expression ParsePrimary()
        {
            var token = CurrentToken;

            // Parenthesized expression
            if (token.Type == TokenType.LParen)
            {
                Consume();
                var inner = ParseExpression();
                if (CurrentToken.Type != TokenType.RParen)
                    throw new ParseException("Expected ')'", CurrentToken.Position, _expression);
                Consume(); // consume ')'
                return new GroupExpression(inner);
            }

            // Variable: IDENTIFIER ("." IDENTIFIER)*
            if (token.Type == TokenType.Identifier)
            {
                var name = Consume().Value;
                while (CurrentToken.Type == TokenType.Op && CurrentToken.Value == ".")
                {
                    Consume(); // consume '.'
                    if (CurrentToken.Type != TokenType.Identifier)
                        throw new ParseException("Expected identifier after '.'", CurrentToken.Position, _expression);
                    name += "." + Consume().Value;
                }
                return new VariableExpression(name);
            }

            // Literals
            if (token.Type == TokenType.String)
            {
                Consume();
                return new LiteralExpression(token.Value);
            }

            if (token.Type == TokenType.Number)
            {
                Consume();
                if (token.Value.Contains('.'))
                {
                    if (double.TryParse(token.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var dbl))
                        return new LiteralExpression(dbl);
                    throw new ParseException($"Invalid numeric literal '{token.Value}'", token.Position, _expression);
                }
                else
                {
                    if (int.TryParse(token.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
                        return new LiteralExpression(integer);
                    throw new ParseException($"Invalid numeric literal '{token.Value}'", token.Position, _expression);
                }
            }

            if (token.Type == TokenType.Bool)
            {
                Consume();
                return new LiteralExpression(token.Value == "true");
            }

            if (token.Type == TokenType.Null)
            {
                Consume();
                return new LiteralExpression(null);
            }

            throw new ParseException($"Unexpected token '{token.Value}'", token.Position, _expression);
        }

        /// <summary>
        /// Consumes the current token and advances to the next.
        /// </summary>
        private Token Consume()
        {
            var token = CurrentToken;
            _pos++;
            return token;
        }
    }
}

/// <summary>
/// Internal token types used by the <see cref="ConditionParser"/> tokenizer.
/// </summary>
internal enum TokenType
{
    Identifier,
    String,
    Number,
    Bool,
    Null,
    Op,
    LParen,
    RParen,
    EndOfInput
}

/// <summary>
/// Internal token produced by the <see cref="ConditionParser"/> tokenizer.
/// </summary>
/// <param name="Type">The type of the token.</param>
/// <param name="Value">The string value of the token.</param>
/// <param name="Position">The character position in the source expression.</param>
internal sealed record Token(TokenType Type, string Value, int Position);
