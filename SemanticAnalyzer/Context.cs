namespace SemanticAnalysis;

public class Context
{
    public Context()
    {
        OuterContext = null;
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
}