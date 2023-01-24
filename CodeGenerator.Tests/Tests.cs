using Shared;
using Shared.Logs;
using Shared.TreeNodes;

namespace CodeGeneration.Tests;

public class Tests
{
    private CodeGenerator _codeGenerator;
    
    [SetUp]
    public void Setup()
    {
        _codeGenerator = new CodeGenerator();
    }

    [Test]

    public void TestFor()
    {
        var instructions = new List<Instruction>
        {
            new()
            {
                Inner = new For()
                {
                    Iterator = new InitializedVariableDeclaration()
                    {
                        Name = new Token(TokenType.Identifier, "i", 0, 0),
                        Type = new TypeNode
                        {
                            IsArrayType = false,
                            Name = new Token(TokenType.Identifier, "int", 0, 0)
                        },
                        Value = new ValueExpression()
                        {
                            Value = new DataNode()
                            {
                                Value = new Token(TokenType.Value, "0",0,0)
                            }
                        }
                    },
                    Comparison = new Comparison()
                    {
                        Left = new ValueExpression()
                        {
                            Value = new DataNode()
                            {
                                Value = new Token(TokenType.Identifier, "i", 0, 0)
                            }
                        },
                        Operator = new DataNode()
                        {
                            Value = new Token(TokenType.Operator, "<", 0,0)
                        },
                        Right = new ValueExpression()
                        {
                            Value = new DataNode()
                            {
                                Value = new Token(TokenType.Value, "10", 0, 0)
                            }
                        }
                    },
                    Expression = new UnaryExpression()
                    {
                        Operator = new DataNode()
                        {
                            Value =  new Token(TokenType.Operator, "++", 0, 0)
                        },
                        Value = new DataNode()
                        {
                            Value =  new Token(TokenType.Identifier, "i", 0, 0)
                        }
                    },
                    Instructions = new List<Instruction>()
                    {
                        new()
                        {
                            Inner = new VariableDeclaration()
                            {
                                Name = new Token(TokenType.Identifier, "a", 0, 0),
                                Type = new TypeNode
                                {
                                    IsArrayType = false,
                                    Name = new Token(TokenType.Identifier, "int", 0, 0)
                                }
                            }
                        }
                    }
                }
            }
        };
        var program = WrapWithProgram(instructions);
        string result = _codeGenerator.Generate(program);
        string expected = File.ReadAllText("../../../TestCases/test1.txt").ReplaceLineEndings("\n");
        Console.WriteLine(result);
        Assert.That(result == expected, Is.True);
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