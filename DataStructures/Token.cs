namespace DataStructures;

public class Token
{
    public readonly TokenType Type;
    public string Value;
    public readonly int ColumnNumber;
    public readonly int LineNumber;

    public Token(TokenType type, string value, int columnNumber, int lineNumber)
    {
        Type = type;
        Value = value;
        ColumnNumber = columnNumber;
        LineNumber = lineNumber;
    }
}