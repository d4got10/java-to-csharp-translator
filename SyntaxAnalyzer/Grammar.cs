using System.Text;
using System.Text.Json;

namespace SyntaxAnalysis;

public class Grammar
{
    public const string EndOfStack = "$";
    public const string Epsilon = "EPSILON";
    
    public string Start { get; private set; } = "";

    private readonly Dictionary<string, List<List<string>>> _productions = new();
    private readonly Dictionary<List<string>, HashSet<string>> _productionFirstSet = new();
    private readonly Dictionary<string, HashSet<string>> _firstSet = new();
    private readonly Dictionary<string, HashSet<string>> _followSet = new();
    private readonly Dictionary<string, Dictionary<string, List<string>>> _parseTableMap = new();

    public void BuildFromFile(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        Build(JsonDocument.Parse(stream));
    }

    public void Process()
    {
        ComputeFirstSet();
        ComputeFollowSet();
        
        CheckFFKeys();
        Validate();
        
        BuildParseTable();
    }
    
    public bool IsTerminal(string token) 
    {
        return token[0] == '\'' && token[^1] == '\'';
    }
    
    public bool IsNonTerminal(string token) 
    {
        return !IsTerminal(token) && !IsSemanticAction(token) && !IsEpsilon(token);
    }
    
    public bool IsSemanticAction(string token) 
    {
        return token[0] == '#' && token[^1] == '#';
    }
        
    public bool IsEpsilon(string token) 
    {
        return token == Epsilon;
    }
    
    private void ComputeFirstSet() 
    {
        // Compute the first set multiple times
        for(int z = 0; z < _productions.Count; z++) 
        {
            // Loop on all definitions
            foreach(var definition in _productions) 
            {
                // If first set for a LHS is not defined
                if(!_firstSet.ContainsKey(definition.Key)) 
                {
                    _firstSet[definition.Key] = new HashSet<string>();
                }
                
                // Loop on all productions of a definition
                foreach(var production in definition.Value) 
                {
                    // If set not created for a production, create it
                    if(!_productionFirstSet.ContainsKey(production)) 
                    {
                        _productionFirstSet[production] = new HashSet<string>();
                    }

                    // Loop on all tokens of a production
                    foreach(var token in production) 
                    {
                        // Check the type of the current token, if terminal or epsilon then add them.
                        // If it's a non-terminal then it needs to be processed.
                        if(IsTerminal(token)) 
                        {
                            string extractedTerminal = ExtractTerminal(token);
                            _productionFirstSet[production].Add(extractedTerminal);
                            _firstSet[definition.Key].Add(extractedTerminal);
                        } 
                        else if(IsEpsilon(token)) 
                        {
                            _productionFirstSet[production].Add(token);
                            _firstSet[definition.Key].Add(token);
                        } 
                        else if(IsNonTerminal(token)) 
                        {
                            HashSet<string>? tokenFirstSet = GetFirstSet(token);

                            // If set was defined previously
                            if(tokenFirstSet != null) 
                            {

                                // If epsilon not found in the first set of the current token or
                                // the current token is the last one in the production, then add all
                                // the tokens to the first set and don't continue evaluating the next tokens
                                // Example 1:
                                //      A -> B C D
                                //      B -> 'T1' | 'T2'
                                //      ...
                                // Then First(A) contains First(B)
                                // Example 2:
                                //      A -> B C D
                                //      B -> EPSILON | 'T1'
                                //      C -> 'T2' | 'T3'
                                //      ...
                                // Then First(A) contains First(B)-{EPSILON} U First(C)
                                // Example 3:
                                //      A -> B C D
                                //      B -> EPSILON | 'T1'
                                //      C -> EPSILON | 'T2'
                                //      D -> EPSILON | 'T3'
                                //      ...
                                // Then First(A) contains First(B)-{EPSILON} U First(C)-{EPSILON} U First(D).
                                // Note that in Example 3, EPSILON will be in First(A)
                                if(!tokenFirstSet.Contains(Epsilon)||
                                        token == LastNonTerminal(production)) 
                                {
                                    foreach (var t in tokenFirstSet)
                                    {
                                        _productionFirstSet[production].Add(t);
                                        _firstSet[definition.Key].Add(t);
                                    }
                                    break;
                                } else {
                                    // Add everything except the epsilon
                                    foreach(var firstSetToken in tokenFirstSet) 
                                    {
                                        if(!IsEpsilon(firstSetToken)) 
                                        {
                                            _productionFirstSet[production].Add(firstSetToken);
                                            _firstSet[definition.Key].Add(firstSetToken);
                                        }
                                    }
                                }
                            }
                            else 
                            {
                                // This is important because the order of non-terminals matter
                                // especially if we don't know if there's epsilon or not
                                // so the next one should not be evaluated until the current is ready
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void ComputeFollowSet() 
    {
        // Add end of stack to the follow set of the start grammar
        _followSet[Start] = new HashSet<string> { EndOfStack };

        // Compute the follow set multiple times
        for(int z=0; z < _productions.Count; z++) 
        {
            // Loop on definitions
            foreach(var definition in _productions) 
            {
                // Loop on productions
                foreach(var production in definition.Value) {

                    // Loop on tokens
                    for(int i=0; i < production.Count; i++) {

                        // Store current token
                        string current = production[i];

                        // If the current token is a non-terminal then iterate on the next token
                        // and add its first set to the follow set of the current token except for
                        // the epsilon value. If the first set had an epsilon value, then the next token
                        // needs to be evaluated as well. This repeats until no epsilon is found or if
                        // there are no more tokens in the production, which in that case the follow set
                        // of the LHS is added to the follow set of the current token.
                        // Example 1
                        //      A -> B C D
                        //      Follow(A) = {'T1'}
                        //      First(C) = {'T2', EPSILON}
                        //      First(D) = {'T3'}
                        // Then Follow(B) contains First(C)-{EPSILON} U First(D)
                        // Example 2
                        //      A -> B C D
                        //      Follow(A) = {'T1'}
                        //      First(C) = {'T2', EPSILON}
                        //      First(D) = {'T3', EPSILON}
                        // Then Follow(B) contains First(C)-{EPSILON} U First(D)-{EPSILON} U Follow(A)
                        if(IsNonTerminal(current)) 
                        {

                            // If set not created, create it
                            if(!_followSet.ContainsKey(current)) 
                            {
                                _followSet[current] = new HashSet<string>();
                            }

                            int j = i;
                            while(++j < production.Count) {

                                // Get token
                                string token = production[j];

                                // Get first set
                                var tokenFirstSet = GetFirstSet(token);

                                // If first set defined
                                if(tokenFirstSet != null) {

                                    // Check if there's an epsilon
                                    bool noEpsilon = true;

                                    // Copy the tokens
                                    foreach(var value in tokenFirstSet) {
                                        if(!IsEpsilon(value)) {
                                            _followSet[current].Add(value);
                                        } else {
                                            noEpsilon = false;
                                        }
                                    }

                                    // Break on epsilon
                                    if(noEpsilon) {
                                        break;
                                    }
                                }
                            }

                            if(j == production.Count) {
                                if(_followSet.ContainsKey(definition.Key)) {
                                    foreach (var t in _followSet[definition.Key])
                                    {
                                        _followSet[current].Add(t);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    private void CheckFFKeys() 
    {
        var diffKeys = new List<string>();
        foreach(var pair in _firstSet) {
            if(!_followSet.ContainsKey(pair.Key)) {
                diffKeys.Add(pair.Key);
            }
        }
        foreach(var pair in _followSet) {
            if(!_firstSet.ContainsKey(pair.Key)) {
                diffKeys.Add(pair.Key);
            }
        }
        if(diffKeys.Count > 0)
        {
            var joined = new StringBuilder();
            for (int i = 0; i < diffKeys.Count - 1; i++)
                joined.Append(diffKeys[i] + ", ");
            joined.Append(diffKeys[^1]);
            
            ThrowError("The following tokens [" + joined + "] either are defined but never " +
            "used in a production, or used in a production but never defined");
        }
    }
    
    private void Validate() {
        // Cond 1
        foreach(var definition in _productions) {
            string leftRecursiveToken = GetLeftRecursion(definition.Key, new HashSet<string>());
            if (!string.IsNullOrWhiteSpace(leftRecursiveToken)) {
                ThrowError("Left hand side: " + leftRecursiveToken + " has a left recursion. Production: " + definition.Key);
            }
        }
        
        // // Cond 2
        // foreach(var definition in _productions) {
        //     var uniqueFirstSetValues = new HashSet<string>();
        //     foreach(var production in _productions[definition.Key]) {
        //         foreach(var token in _productionFirstSet[production]) {
        //             if(uniqueFirstSetValues.Contains(token)) {
        //                 ThrowError("The first set of the rules of the non-terminal: " +
        //                            definition.Key + " intersect at " + token);
        //             } else {
        //                 uniqueFirstSetValues.Add(token);
        //             }
        //         }
        //     }
        // }

        // Cond 3
        foreach(var firstPair in _firstSet) 
        {
            if(firstPair.Value.Contains(Epsilon)) 
            {
                var intersection = new List<string>();
                foreach(var token in firstPair.Value) 
                {
                    if(token != Epsilon && _followSet[firstPair.Key].Contains(token)) 
                    {
                        intersection.Add(token);
                    }
                }
                if(intersection.Count > 0) {
                    ThrowError("The first and follow sets of the non-terminal intersect");
                }
            }
        }
    }
    
    private void BuildParseTable() {

        // Loop on all definitions
        foreach(var definition in _productions) {

            // Create map
            _parseTableMap[definition.Key] = new Dictionary<string, List<string>>();

            // Loop on all productions
            foreach(var production in definition.Value) {

                // Get the first set of the production
                HashSet<string> pFirstSet = _productionFirstSet[production];

                // The map key will always be unique since the grammar
                // passed the LL condition tests

                // Couple each token in the first set to the production, except for epsilon
                bool hasEpsilon = false;
                foreach(string token in pFirstSet) {
                    if(!IsEpsilon(token)) {
                        _parseTableMap[definition.Key][token] = production;
                    } else {
                        hasEpsilon = true;
                    }
                }

                // Check if first set contains epsilon, then map the follow set
                if(hasEpsilon) {
                    // Get the follow set of the production
                    var pFollowSet = _followSet[definition.Key];

                    // Couple each token in the follow set to the production
                    foreach(var token in pFollowSet) {
                        _parseTableMap[definition.Key][token] = production;
                    }
                }
            }
        }
    }
    
    private void Build(JsonDocument document)
    {
        foreach (var rule in document.RootElement.EnumerateObject())
        {
            string nonTerminal = rule.Name;
            var array = rule.Value.EnumerateArray().ToArray();
            
            if(array.Length == 0) 
                ThrowError("Non terminal " + nonTerminal + " cannot have an empty production array");

            if (string.IsNullOrWhiteSpace(nonTerminal)) 
                ThrowError("A non-terminal key cannot be empty");

            if(_productions.ContainsKey(nonTerminal))
                ThrowError("All definition for the non-terminal " + nonTerminal +
                         " should be grouped in the array value");

            _productions[nonTerminal] = new List<List<string>>();

            if (string.IsNullOrWhiteSpace(Start))
                Start = nonTerminal;

            foreach (var element in array)
            {
                var production = element.GetString();
                production = production?.Trim();
                
                if(string.IsNullOrWhiteSpace(production))
                    ThrowError("A production cannot be empty.");

                string processString = production!;
                _productions[nonTerminal].Add(new List<string>());

                foreach (var word in processString.Split(' '))
                {
                    if((IsTerminal(word) || IsEpsilon(word)) &&
                       !IsEmptyWithIgnoreExceptions(_productions[nonTerminal][^1])) 
                    {
                        ThrowError("A production containing a terminal or an epsilon token " + 
                                 "cannot be followed or preceded by other tokens.");
                    }
                    _productions[nonTerminal][^1].Add(word);
                }
            }
        }
    }

    private void ThrowError(string error)
    {
        throw new Exception(error);
    }
    

    private bool IsEmptyWithIgnoreExceptions(List<string> target)
    {
        return !target.Any(IsSemanticAction);
    }
    
    public string ExtractTerminal(string terminal) {
        if(terminal.Length > 2 && terminal[0] == '\'' && terminal[^1] == '\'') {
            return terminal.Substring(1, terminal.Length-2);
        }
        ThrowError("String '" + terminal + "' is not a terminal");
        throw new Exception();
    }
    
    public List<string>? GetParseTable(string nonTerminal, string input) {
        if(_parseTableMap.ContainsKey(nonTerminal) && _parseTableMap[nonTerminal].ContainsKey(input)) 
        {
            return _parseTableMap[nonTerminal][input];
        }
        return null;
    }
    
    public HashSet<string>? GetFirstSet(string token) {
        if(IsNonTerminal(token) && _firstSet.ContainsKey(token)) {
            return _firstSet[token];
        }
        return null;
    }

    public HashSet<string>? GetFollowSet(string token) {
        if(IsNonTerminal(token) && _followSet.ContainsKey(token)) {
            return _followSet[token];
        }
        return null;
    }
    
    public string LastNonTerminal(List<string> production)
    {
        var last = production.LastOrDefault(IsNonTerminal);
        if (last == null) 
            ThrowError("Production does not contain any non-terminal");
        return last;
    }
    
    public string GetLeftRecursion(string token, HashSet<string> visited)
    {
        // If visited more than once
        if(visited.Contains(token)) {
            return token;
        }

        // Mark non terminal as visited
        visited.Add(token);

        foreach(var production in _productions[token]) {
            foreach(string syntaxToken in production) {
                if(IsNonTerminal(syntaxToken)) {

                    // Check recursively for left-recursion
                    string result = GetLeftRecursion(syntaxToken, visited);
                    if(!string.IsNullOrWhiteSpace(result)) {
                        return result;
                    }

                    // Stop checking when the first set of non-terminal does not contain epsilon
                    if(!_firstSet[syntaxToken].Contains(Epsilon)) {
                        break;
                    }
                } else if(IsTerminal(syntaxToken)) {
                    // Stop checking if token is a terminal
                    break;
                }
            }
        }

        visited.Remove(token);
        // No left recursion found
        return "";
    }
}