namespace SemanticAnalysis;

public class Context
{
    public Context(HashSet<string> definedWords, Dictionary<string, string> definedTypes)
    {
        OuterContext = null;
        _usedWords = definedWords;
        _wordTypes = definedTypes;
    }
    
    public Context(Context outerContext)
    {
        OuterContext = outerContext;
    }
    
    public readonly Context? OuterContext;

    private readonly HashSet<string> _usedWords = new();
    private readonly Dictionary<string, string> _wordTypes = new();
    
    public bool CheckWord(string word)
    {
        if (_usedWords.Contains(word))
            return true;

        return OuterContext != null && OuterContext.CheckWord(word);
    }

    public void ReserveWord(string word)
    {
        if (WordHasType(word))
            throw new InvalidOperationException($"Word {word} was already reserved");
        
        _usedWords.Add(word);
    }

    public bool WordHasType(string word)
    {
        if (_wordTypes.ContainsKey(word))
            return true;

        return OuterContext != null && OuterContext.WordHasType(word);
    }
    
    public string GetWordType(string word)
    {
        if(_wordTypes.ContainsKey(word))
            return _wordTypes[word];
        return OuterContext.GetWordType(word);
    }

    public void SetWordType(string word, string type)
    {
        ReserveWord(word);
        _wordTypes[word] = type;
    }

    public string GetValueType(string value)
    {
        if (value.StartsWith("\"") && value.EndsWith("\"")) return "string";
        if (value.StartsWith("\'") && value.EndsWith("\'")) return "char";
        if (value.Contains(".")) return "double";
        if (value.EndsWith("f")) return "float";
        if (value == "true" || value == "false") return "bool";
        if (int.TryParse(value, out _)) return "int";

        throw new ArgumentOutOfRangeException(nameof(value), $"Unknown value type {value}");
    }
}