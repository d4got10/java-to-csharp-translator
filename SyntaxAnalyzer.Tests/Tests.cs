using Shared;
using Shared.Logs;
using Shared.TreeNodes;

namespace SyntaxAnalysis.Tests;

public class Tests
{
    private const string PathToSolutionDirectory = "../../../../";
    
    private SyntaxAnalyzer _syntaxAnalyzer;
    private LazyLogger _logger;
    
    [SetUp]
    public void Setup()
    {
        _logger = new LazyLogger();
        _syntaxAnalyzer = new SyntaxAnalyzer(
            PathToSolutionDirectory + "Grammar.json", 
            PathToSolutionDirectory + "SyntaxErrors.json", 
            _logger);
    }

    [Test]
    public void TestDeclaration()
    {
        var testBody = CreateEmptyTestBody();
        
        testBody = testBody.Concat(CreateDeclaration("int", "i"));
        testBody = testBody.Concat(CreateDeclaration("double", "d"));
        testBody = testBody.Concat(CreateDeclaration("string", "s"));
        testBody = testBody.Concat(CreateDeclaration("char", "c"));

        var program = WrapWithMainProgram(testBody);

        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestDeclarationWithValueInitialization()
    {
        var testBody = CreateEmptyTestBody();
        
        testBody = testBody.Concat(CreateDeclarationWithValueInitialization("int", "ii", "420"));
        testBody = testBody.Concat(CreateDeclarationWithValueInitialization("double", "dd", "1.488"));
        testBody = testBody.Concat(CreateDeclarationWithValueInitialization("string", "ss", "\"str\""));
        testBody = testBody.Concat(CreateDeclarationWithValueInitialization("char", "cc", "\'c\'"));
        
        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestWhile()
    {
        var testBody = new Token[]
        {
            new(TokenType.Keyword, "while", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, "==", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
        };

        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestFor()
    {
        var testBody = new Token[]
        {
            new(TokenType.Keyword, "for", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "int", 0, 0),
            new(TokenType.Identifier, "i", 0, 0),
            new(TokenType.Operator, "=", 0, 0),
            new(TokenType.Identifier, "0", 0, 0),
            new(TokenType.Semicolon, ";", 0, 0),
            new(TokenType.Identifier, "i", 0, 0),
            new(TokenType.Operator, "<", 0, 0),
            new(TokenType.Value, "10", 0, 0),
            new(TokenType.Semicolon, ";", 0, 0),
            new(TokenType.Identifier, "i", 0, 0),
            new(TokenType.Operator, "++", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
        };

        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestIf()
    {
        var testBody = new Token[]
        {
            new(TokenType.Keyword, "if", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, "==", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
        };

        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }
    
    [Test]
    public void TestIfElse()
    {
        var testBody = new Token[]
        {
            new(TokenType.Keyword, "if", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, "==", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
            new(TokenType.Keyword, "else", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
        };

        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }
    
    [Test]
    public void TestIfElseIfElse()
    {
        var testBody = new Token[]
        {
            new(TokenType.Keyword, "if", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, "==", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
            new(TokenType.Keyword, "else", 0, 0),
            new(TokenType.Keyword, "if", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, "==", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
            new(TokenType.Keyword, "else", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
        };

        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestDoWhile()
    {
        var testBody = new Token[]
        {
            new(TokenType.Keyword, "do", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
            new(TokenType.Keyword, "while", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, "==", 0, 0),
            new(TokenType.Identifier, "id", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.Semicolon, ";", 0, 0),
        };

        var program = WrapWithMainProgram(testBody);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestConsoleWriting()
    {
        var body = CreateEmptyTestBody();

        body = body.Concat(CreateConsoleWrite("andrey privet"));
        
        var program = WrapWithMainProgram(body);

        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    [Test]
    public void TestAssignment()
    {
        var body = CreateEmptyTestBody();

        body = body.Concat(CreateAssignment("a", "123"));
        body = body.Concat(CreateAssignment("b", "4.2"));
        body = body.Concat(CreateAssignment("c", "\"mystring\""));
        
        var program = WrapWithMainProgram(body);
        
        Assert.That(_syntaxAnalyzer.Parse(program, new SemanticMessenger()), Is.True);
    }

    private IEnumerable<Token> CreateConsoleWrite(string argument)
    {
        return new Token[]
        {
            new(TokenType.Identifier, "System", 0, 0),
            new(TokenType.Operator, ".", 0, 0),
            new(TokenType.Identifier, "out", 0, 0),
            new(TokenType.Operator, ".", 0, 0),            
            new(TokenType.Identifier, "println", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, $"\"{argument}\"", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.Semicolon, ";", 0, 0)
        };
    }

    private IEnumerable<Token> CreateDeclaration(string type, string identifier)
    {
        return new Token[]
        {
            new(TokenType.Identifier, type, 0, 0),
            new(TokenType.Identifier, identifier, 0, 0),
            new(TokenType.Semicolon, ";", 0, 0)
        };
    }

    private IEnumerable<Token> CreateAssignment(string variableName, string value)
    {
        return new Token[]
        {
            new(TokenType.Identifier, variableName, 0, 0),
            new(TokenType.Operator, "=", 0, 0),
            new(TokenType.Value, value, 0, 0),
            new(TokenType.Semicolon, ";", 0, 0)
        };
    }

    private IEnumerable<Token> CreateEmptyTestBody() => Enumerable.Empty<Token>();

    private IEnumerable<Token> CreateDeclarationWithValueInitialization(string type, string identifier, string value)
    {
        return new Token[]
        {
            new(TokenType.Identifier, type, 0, 0),
            new(TokenType.Identifier, identifier, 0, 0),
            new(TokenType.Operator, "=", 0, 0),
            new(TokenType.Value, value, 0, 0),
            new(TokenType.Semicolon, ";", 0, 0)
        };
    }

    private IEnumerable<Token> WrapWithMainProgram(IEnumerable<Token> body)
    {
        var prefix = new Token[]
        {
            new(TokenType.AccessModifier, "public", 0, 0),
            new(TokenType.Keyword, "class", 0, 0),
            new(TokenType.Identifier, "Program", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
            new(TokenType.AccessModifier, "public", 0, 0),
            new(TokenType.Keyword, "static", 0, 0),
            new(TokenType.Type, "void", 0, 0),
            new(TokenType.Identifier, "Main", 0, 0),
            new(TokenType.Operator, "(", 0, 0),
            new(TokenType.Identifier, "String", 0, 0),
            new(TokenType.Operator, "[", 0, 0),
            new(TokenType.Operator, "]", 0, 0),
            new(TokenType.Identifier, "args", 0, 0),
            new(TokenType.Operator, ")", 0, 0),
            new(TokenType.OpenBracket, "{", 0, 0),
        };
        var postfix = new Token[]
        {
            new(TokenType.CloseBracket, "}", 0, 0),
            new(TokenType.CloseBracket, "}", 0, 0),
        };
        return prefix.Concat(body).Concat(postfix);
    }
}