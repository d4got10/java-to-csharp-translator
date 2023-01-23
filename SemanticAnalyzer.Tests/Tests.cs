using Shared;
using Shared.Logs;
using Shared.TreeNodes;

namespace SemanticAnalysis.Tests;

public class Tests
{
    private LazyLogger _lazyLogger;
    private SemanticAnalyzer _semanticAnalyzer;
    
    [SetUp]
    public void Setup()
    {
        _lazyLogger = new LazyLogger();
        _semanticAnalyzer = new SemanticAnalyzer(_lazyLogger);
    }

    [Test]
    public void TestIncompatibleTypes()
    {
        var instructions = new List<Instruction>
        {
            new()
            {
                Inner = new InitializedVariableDeclaration()
                {
                    Name = new Token(TokenType.Identifier, "a", 0, 0),
                    Type = new TypeNode
                    {
                        IsArrayType = false,
                        Name = new Token(TokenType.Identifier, "int", 0, 0)
                    },
                    Value = new ValueExpression
                    {
                        Value = new DataNode
                        {
                            Value = new Token(TokenType.Value, "\"andrew privet\"", 0, 0)
                        }
                    }
                }
            },
        };

        var program = WrapWithProgram(instructions);
        
        bool result = _semanticAnalyzer.Analyze(program);
        Console.WriteLine(_lazyLogger.GetLogs());
        Assert.That(result, Is.False);
    }

    [Test]
    public void TestUndeclaredVariable()
    {
        var instruction = new Instruction
        {
            Inner = new ExpressionAssignment
            {
                VariableName = new Token(TokenType.Identifier, "a", 0, 0),
                Value = new ValueExpression
                {
                    Value = new DataNode
                    {
                        Value = new Token(TokenType.Identifier, "5", 0, 0)
                    }
                }
            }
        };

        var program = WrapWithProgram(new List<Instruction>{ instruction });
        
        bool result = _semanticAnalyzer.Analyze(program);
        Console.WriteLine(_lazyLogger.GetLogs());
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void TestDeclaredVariable()
    {
        var instructions = new List<Instruction>
        {
            new()
            {
                Inner = new VariableDeclaration
                {
                    Name = new Token(TokenType.Identifier, "a", 0, 0),
                    Type = new TypeNode
                    {
                        IsArrayType = false,
                        Name = new Token(TokenType.Identifier, "int", 0, 0)
                    }
                }
            },
            new()
            {
                Inner = new ExpressionAssignment
                {
                    VariableName = new Token(TokenType.Identifier, "a", 0, 0),
                    Value = new ValueExpression
                    {
                        Value = new DataNode
                        {
                            Value = new Token(TokenType.Value, "5", 0, 0)
                        }
                    }
                }
            }
        };

        var program = WrapWithProgram(instructions);

        bool result = _semanticAnalyzer.Analyze(program);
        Console.WriteLine(_lazyLogger.GetLogs());
        Assert.That(result, Is.True);
    }

    private ClassDeclaration WrapWithProgram(List<Instruction> instructions)
    {
        return new ClassDeclaration
        {
            AccessModifier = new Token(TokenType.AccessModifier, "public", 0, 0),
            Functions = new List<FunctionDeclaration>()
            {
                new()
                {
                    AccessModifier = new Token(TokenType.AccessModifier, "public", 0, 0),
                    IsStatic = true,
                    Data = new VariableDeclaration
                    {
                        Name = new Token(TokenType.Identifier, "Main", 0, 0),
                        Type = new TypeNode
                        {
                            Name = new Token(TokenType.Identifier, "void", 0, 0),
                            IsArrayType = false
                        }
                    },
                    Parameters = new Parameters
                    {
                        Variables  = new List<VariableDeclaration>
                        {
                            new()
                            {
                                Name = new Token(TokenType.Identifier, "args", 0, 0),
                                Type = new TypeNode
                                {
                                    Name = new Token(TokenType.Identifier, "String", 0, 0),
                                    IsArrayType = true
                                }
                            }
                        }
                    },
                    Instructions = instructions
                }
            },
            Name = new Token(TokenType.Identifier, "Program", 0, 0)
        };
    }
}