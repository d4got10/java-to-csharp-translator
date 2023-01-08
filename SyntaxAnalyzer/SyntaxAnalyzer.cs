using System.Collections;
using System.Text.RegularExpressions;
using DataStructures;

namespace SyntaxAnalysis;

public class SyntaxAnalyzer
{
    private List<Token> _tokens = new();
    private SyntaxMessages _messages;
    
    public bool Parse(IEnumerable<Token> tokens)
    {
        _tokens = tokens.ToList();

        var stack = new Stack<string>();
        var semanticStack = new Stack<string>();
        int inputIndex = 0;
        bool success = true;

        var token = GetNextToken(ref inputIndex);

        _messages = new SyntaxMessages();
        _messages.LoadMessagesFromFile("syntax_errors.json");
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
                if(grammar.ExtractTerminal(top) == GetName(token)) 
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
                semanticStack.Push(GetName(_tokens[inputIndex - 2]));
                stack.Pop();
            } 
            else 
            { // It is a non-terminal

                // Get record from the parse table
                List<string> production = grammar.GetParseTable(top, GetName(token));
                
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
                    Console.WriteLine(GenerateErrorMessage("fileName", top, _tokens, inputIndex - 1));
                    success = false;

                    // If terminal is in the follow set or there is no more input to process,
                    // then pop the parse stack
                    var followSet = grammar.GetFollowSet(top);
                    if(followSet != null && followSet.Contains(GetName(token)) ||
                       GetName(token) == Grammar.EndOfStack) {
                        stack.Pop();
                    } else {
                        token = GetNextToken(ref inputIndex);
                    }
                }
            }
        }

        if(stack.Peek() != Grammar.EndOfStack) 
        {
            // Generate error message in the first parsing phase
            Console.WriteLine(GenerateErrorMessage("fileName", stack.Peek(), _tokens, inputIndex-1));
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

    private string GenerateErrorMessage(string fileName, string nonTerminal, List<Token> tokens, int index) 
    {
        // Load error message
        string message = _messages.GetErrorMessage(nonTerminal, GetName(_tokens[index]));
        string messageCopy = message;

        // Match error messages
        var exp = new Regex("(\\$\\{lexical(\\.(?:next|previous))*\\.(?:value|name|line|column)\\})");
        var matches = exp.Matches(messageCopy);
        foreach(Match match in matches) {
            // Split by dot
            string matchValue = match.Value;
            var words = matchValue.Split('.');

            // Navigate to the correct token
            int newIndex = index;
            for(int i = 1; i < words.Length - 1; i++) {
                if(words[i] == "next") {
                    newIndex++;
                } else if(words[i] == "previous") {
                    newIndex--;
                }
            }

            string newValue = "";

            // If new index is not found
            if(newIndex < 0 || newIndex >= _tokens.Count) {
                newValue = "undefined";
            } else {
                string type = words[^1].Substring(0, words[^1].Length - 1);
                if (type == "value") {
                    newValue = _tokens[newIndex].Value;
                } else if (type == "name") {
                    newValue = GetName(_tokens[newIndex]);
                } else if (type == "line") {
                    newValue = _tokens[newIndex].LineNumber.ToString();
                } else if (type == "column") {
                    newValue = _tokens[newIndex].ColumnNumber.ToString();
                }
            }

            message = message.Replace(matchValue, newValue);
        }

        return message;
    }
    
    private string GetName(Token? token)
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