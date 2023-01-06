using DataStructures;

namespace SyntaxAnalysis;

public class SyntaxAnalyzer
{
    private List<Token> _tokens = new();
    
    public bool Parse(IEnumerable<Token> tokens)
    {
        _tokens = tokens.ToList();

        var stack = new Stack<string>();
        int inputIndex = 0;
        bool success = true;

        var token = GetNextToken(ref inputIndex);
        
        var grammar = new Grammar(); 
        grammar.BuildFromFile("grammar.txt");
        grammar.Process();
        
        stack.Push(Grammar.EndOfStack);
        stack.Push(grammar.Start);

        // While more non-terminals are in the parse stack
        while(stack.Peek() != Grammar.EndOfStack) 
        {
            // Get the top token from the parser stack
            var top = stack.Peek();

            // Check the type of the token
            if(grammar.IsTerminal(top)) 
            {
                // If there is a match
                if(grammar.ExtractTerminal(top) == GetValue(token)) 
                {
                    // Start over by scanning new input and processing a new top
                    stack.Pop();
                    token = GetNextToken(ref inputIndex);
                } 
                else 
                {
                    throw new Exception(
                            "Failed to process the input: " + 
                                    "In the Syntax analysis phase, the stack top terminal " +
                                    "and the lexical input terminal token did not match! " +
                                    "Please report this problem.");
                }
            } 
            else if(grammar.IsSemanticAction(top)) 
            {
                stack.Pop();
            } 
            else 
            { // It is a non-terminal

                // Get record from the parse table
                List<string> production = grammar.GetParseTable(top, GetValue(token));
                
                // Check if the record exists or it is an error
                if(production != null) 
                {
                    stack.Pop();

                    // Insert the new production right to left
                    for(int i = 0; i < production.Count; ++i) 
                    {
                        // If not epsilon
                        if(!grammar.IsEpsilon(production[^(1+i)]))
                        {
                            stack.Push(production[^(1+i)]);
                        }
                    }

                } 
                else 
                { // Error found
                    // Generate error message in the first parsing phase
                    // if(!m_silentSyntaxErrorMessages) {
                    //     std::cerr << generateErrorMessage(fileName, top, lexicalTokens, inputIndex-1)
                    //               << std::endl;
                    // }
                    success = false;

                    // If terminal is in the follow set or there is no more input to process,
                    // then pop the parse stack
                    var followSet = grammar.GetFollowSet(top);
                    if(followSet != null && followSet.Contains(GetValue(token)) ||
                       stack.Peek() == Grammar.EndOfStack) {
                        stack.Pop();
                    } else {
                        token = GetNextToken(ref inputIndex);
                    }

                    // Broadcast error
                    // if(m_onSyntaxError) {
                    //     this->m_onSyntaxError();
                    // }
                }
            }
        }

        if(stack.Peek() != Grammar.EndOfStack) 
        {
            // Generate error message in the first parsing phase
            // if(!m_silentSyntaxErrorMessages) {
            //     std::cerr << generateErrorMessage(fileName, parseStack.top(), lexicalTokens, inputIndex-1) << std::endl;
            // }
            Console.WriteLine($"Stack is not empty: {stack.Peek()}");
            success = false;
        }

        // BOOST_LOG(ecc_logger::get()) << "Finished parsing the lexical tokens [PHASE " << m_phase << "]";
        // BOOST_LOG(ecc_logger::get()) << (success ? "SUCCESS" : "FAILURE");
        // BOOST_LOG(ecc_logger::get()) << "----------";
        return success;
    }

    private Token? GetNextToken(ref int index)
    {
        return index < _tokens.Count 
            ? _tokens[index++] 
            : null;
    }

    private const string Eps = "~"; 

    private Dictionary<string, string[]> GetRules()
    {
        return new Dictionary<string, string[]>
        {   
            {"E", new[]{"TE\'"}},
            {"E\'", new[]{"+TE\'", Eps}},
            {"T", new []{"FT\'"}},
            {"T\'", new []{"*FT\'", Eps}},
            {"F", new []{"id", "(E)"}}
        };
    }

    private Dictionary<(string, string), string> CreateTable(Dictionary<string, string[]> rules)
    {
        var dict = new Dictionary<(string, string), string>();

        var first = new Dictionary<string, HashSet<string>>();
        foreach (var (left, rights) in rules)
        {
            foreach (var right in rights)
            {
                if (right == Eps)
                {
                    first[left].Add(right);
                }
            }
        }
        
        return dict;
    }

    private string GetValue(Token? token)
    {
        if (token == null)
            return Grammar.EndOfStack;
        
        return token.Type switch
        {
            TokenType.Keyword => throw new ArgumentOutOfRangeException(),
            TokenType.Operator => token.Value switch
            {
                "+" => "T_PLUS",
                "-" => "T_MINUS",
                "*" => "T_MULTIPLY",
                "(" => "T_OPEN_PAR",
                ")" => "T_CLOSE_PAR",
                _ => throw new ArgumentOutOfRangeException()
            },
            TokenType.Type => throw new ArgumentOutOfRangeException(),
            TokenType.Identifier => throw new ArgumentOutOfRangeException(),
            TokenType.Semicolon => "T_SEMICOLON",
            TokenType.AccessModifier => throw new ArgumentOutOfRangeException(),
            TokenType.Value => "T_INTEGER",
            TokenType.OpenBracket => throw new ArgumentOutOfRangeException(),
            TokenType.CloseBracket => throw new ArgumentOutOfRangeException(),
            TokenType.Comma => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}