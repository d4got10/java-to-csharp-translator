using System.Text.Json;

namespace SyntaxAnalysis;

public class SyntaxMessages
{
    // Reserved token names
    private const string DefaultTerminalAndNonTerminal = ":any";
    private const string ErrorMessages = "error_messages";
    private const string DefaultMessage = "default_message";
    private const string NonTerminal = "non_terminal";
    private const string Terminal = "terminal";
    private const string Message = "message";
    
    private readonly Dictionary<string, Dictionary<string, string>> _errorMessages = new();
    
    private string _defaultMessage;
    
    public void LoadMessagesFromFile(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        LoadMessages(JsonDocument.Parse(stream));
    }

    public string GetErrorMessage(string nonTerminal, string terminal) {

        // If non-terminal registered
        if(_errorMessages.ContainsKey(nonTerminal)) 
        {
            // If terminal registered in that non-terminal
            if(_errorMessages[nonTerminal].ContainsKey(terminal)) 
            {
                return _errorMessages[nonTerminal][terminal];
            } 
            if(_errorMessages[nonTerminal].ContainsKey(DefaultTerminalAndNonTerminal)) 
            {
                return _errorMessages[nonTerminal][DefaultTerminalAndNonTerminal];
            }
        } 
        else if(_errorMessages.ContainsKey(DefaultTerminalAndNonTerminal) &&
                _errorMessages[DefaultTerminalAndNonTerminal].ContainsKey(terminal)) 
        {
            return _errorMessages[DefaultTerminalAndNonTerminal][terminal];
        }
        return _defaultMessage;
    }

    private void LoadMessages(JsonDocument document) 
    {
        // Default message
        _defaultMessage = document.RootElement.GetProperty(DefaultMessage).GetString();
        var errorMessages = document.RootElement.GetProperty(ErrorMessages).EnumerateArray();
        // Loop on all messages
        foreach(var errorMessage in errorMessages) 
        {

            // Get error block information
            string nonTerminal = errorMessage.GetProperty(NonTerminal).GetString();
            string terminal = errorMessage.GetProperty(Terminal).GetString();
            string message = errorMessage.GetProperty(Message).GetString();

            // Verify duplicates
            if(_errorMessages.ContainsKey(nonTerminal) && _errorMessages[nonTerminal].ContainsKey(terminal)) 
            {
                throw new Exception("Message with non-terminal: " + nonTerminal +
                                         " and terminal: " + terminal + " is defined multiple times");
            }

            if (!_errorMessages.ContainsKey(nonTerminal))
            {
                _errorMessages[nonTerminal] = new Dictionary<string, string>();
            }
            _errorMessages[nonTerminal][terminal] = message;
        }
    }
}