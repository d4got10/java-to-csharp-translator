namespace LexicalAnalysis;

public static class TokenTypeHashSets
{
    public static readonly HashSet<string> accessModifiers = new HashSet<string>()
    {
        "public",
        "private",
        "protected"
    };
    
    public static readonly HashSet<string> keyWords = new HashSet<string>()
    {
        "abstract",
        "break",
        "case",
        "catch",
        "class",
        "continue",
        "default",
        "do",
        "else",
        "enum",
        "extends",
        "for",
        "if",
        "implements",
        "import",
        "instanceof",
        "interface",
        "new",
        "package",
        "return",
        "static",
        "super",
        "switch",
        "synchronized",
        "this",
        "throw",
        "throws",
        "transient",
        "try",
        "while"
    };
    
    public static readonly HashSet<string> dataTypes = new HashSet<string>()
    {
        "byte",
        "short",
        "int",
        "long",
        "float",
        "double",
        "boolean",
        "char",
        "string",
        "String",
        "void"
    };

    public static readonly HashSet<string> operators = new HashSet<string>()
    {
        "=",
        "==",
        ">=",
        "<=",
        "+",
        "-",
        "*",
        "/",
        "%",
        "++",
        "--",
        "+=",
        "-=",
        "*=",
        "/=",
        "%=",
        "&=",
        "|=",
        "^=",
        ">>=",
        "<<=",
        "!=",
        "&&",
        "||",
        "!",
        "(",
        ")",
        "[",
        "]",
        "..",
        ".",
        "?.",
        "^",
        "<",
        ">"
    };

}