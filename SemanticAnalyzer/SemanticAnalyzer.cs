using Shared;
using Shared.Logs;
using Shared.TreeNodes;

namespace SemanticAnalysis;

public class SemanticAnalyzer
{
    public SemanticAnalyzer(ILogger logger)
    {
        _logger = logger;
    }

    private Context _context = new(new HashSet<string>
        {
            "System.out.println",
            "String"
        }, 
        new Dictionary<string, string>());

    private readonly ILogger _logger;

    public bool Analyze(Node root)
    {
        return root switch
        {
            DataNode dataNode => AnalyzeDataNode(dataNode),
            ClassDeclaration classDeclaration => AnalyzeClassDeclaration(classDeclaration),
            FunctionDeclaration functionDeclaration => AnalyzeFunctionDeclaration(functionDeclaration),
            InitializedVariableDeclaration initializedVariableDeclaration => AnalyzeInitializedVariableDeclaration(initializedVariableDeclaration),
            VariableDeclaration variable => AnalyzeVariableDeclaration(variable),
            FunctionCall functionCall => AnalyzeFunctionCall(functionCall),
            Instruction instruction => AnalyzeInstruction(instruction),
            While @while => AnalyzeWhile(@while),
            For @for => AnalyzeFor(@for),
            Comparison comparison => AnalyzeComparison(comparison),
            UnaryAssignment unaryAssignment => AnalyzeUnaryAssignment(unaryAssignment),
            ExpressionAssignment expressionAssignment => AnalyzeExpressionAssignment(expressionAssignment),
            Expression expression => AnalyzeExpression(expression),
            If @if => AnalyzeIf(@if),
            ElseIf elseIf => AnalyzeElseIf(elseIf),
            Else @else => AnalyzeElse(@else),
            NoTail => true,
            DoWhile doWhile => AnalyzeDoWhile(doWhile),
            _ => throw new Exception("Unknown node type: " + root.GetType())
        };
    }

    private bool AnalyzeExpression(Expression node)
    {
        switch (node)
        {
            case ValueExpression value:
                if (value.Value.Value.Type == TokenType.Identifier &&!_context.CheckWord(value.Value.Value.Value))
                {
                    LogUnknownNameError(value.Value.Value);
                    return false;
                }
                return true;
            case UnaryExpression unary:
                return true;
            case BinaryExpression binary:
                return true;
            default:
                return false;
        }
    }

    private bool AnalyzeFunctionCall(FunctionCall node)
    {
        if (!_context.CheckWord(node.Name.Value.Value))
        {
            LogUnknownNameError(node.Name.Value);
            return false;
        }

        bool ok = true;
        foreach (Expression expression in node.Arguments.Expressions)
        {
            ok &= Analyze(expression);
        }
        return ok;
    }

    private bool AnalyzeInitializedVariableDeclaration(InitializedVariableDeclaration node)
    {
        if (!_context.CheckWord(node.Name.Value))
        {
            _context.SetWordType(node.Name.Value, node.Type.Name.Value);
            return Analyze(new ExpressionAssignment
            {
                VariableName = node.Name,
                Value = node.Value
            });
        }
        LogNameAlreadyUsedError(node.Name);

        return false;
    }

    private bool AnalyzeIf(If node)
    {
        bool ok = true;
        ok &= Analyze(node.Comparison);
        foreach (Instruction instruction in node.Instructions)
        {
            ok &= Analyze(instruction);
        }
        ok &= Analyze(node.Tail);
        return ok;
    }

    private bool AnalyzeElseIf(ElseIf node)
    {
        bool ok = true;
        ok &= Analyze(node.Comparison);
        foreach (Instruction instruction in node.Instructions)
        {
            ok &= Analyze(instruction);
        }
        ok &= Analyze(node.Tail);
        return ok;
    }
    
    private bool AnalyzeElse(Else node)
    {
        bool ok = true;
        foreach (Instruction instruction in node.Instructions)
        {
            ok &= Analyze(instruction);
        }
        return ok;
    }
    
    private bool AnalyzeDataNode(DataNode node)
    {
        if (!_context.CheckWord(node.Value.Value))
        {
            LogUnknownNameError(node.Value);
            return false;
        }
        return true;
    }

    private bool AnalyzeFor(For node)
    {
        bool ok = true;
        _context = new Context(_context);
        ok &= Analyze(node.Iterator);
        ok &= Analyze(node.Comparison);
        ok &= Analyze(node.Expression);
        foreach(var instruction in node.Instructions)
            ok &= Analyze(instruction);
        _context = _context.OuterContext!;
        return ok;
    }

    private bool AnalyzeUnaryAssignment(UnaryAssignment node)
    {
        if (!_context.CheckWord(node.VariableName.Value))
        {
            LogUnknownNameError(node.VariableName);
            return false;
        }
        return true;
    }

    private bool AnalyzeExpressionAssignment(ExpressionAssignment node)
    {
        if (!_context.CheckWord(node.VariableName.Value))
        {
            LogUnknownNameError(node.VariableName);
            return false;
        }

        if (!TryGetExpressionType(node.Value, out var expressionType))
        {
            return false;
        }
        var type = _context.GetWordType(node.VariableName.Value);
        if (!CanBeConverted(expressionType, type))
        {
            LogIncompatibleConversionTypes(type, expressionType, node.VariableName);
            return false;
        }

        return true;
    }

    private bool AnalyzeComparison(Comparison node)
    {
        bool ok = true;
        ok &= Analyze(node.Left);
        ok &= Analyze(node.Right);
        ok &= TryGetExpressionType(node.Left, node.Right, out _);

        return ok;
    }

    private bool AnalyzeWhile(While node)
    {
        bool ok = true;
        _context = new Context(_context);
        ok &= Analyze(node.Comparison);
        foreach(var instruction in node.Instructions)
            ok &= Analyze(instruction);
        _context = _context.OuterContext!;
        return ok;
    }
    
    private bool AnalyzeDoWhile(DoWhile node)
    {
        bool ok = true;
        _context = new Context(_context);
        ok &= Analyze(node.Comparison);
        foreach(var instruction in node.Instructions)
            ok &= Analyze(instruction);
        _context = _context.OuterContext!;
        return ok;
    }

    private bool AnalyzeInstruction(Instruction node)
    {
        return Analyze(node.Inner);
    }

    private bool AnalyzeFunctionDeclaration(FunctionDeclaration node)
    {
        bool ok = true;
        ok &= Analyze(node.Data);
        
        _context = new Context(_context);
        foreach (var parameter in node.Parameters.Variables) 
            ok &= Analyze(parameter);

        foreach (var instruction in node.Instructions) 
            ok &= Analyze(instruction);
        _context = _context.OuterContext!;
        return ok;
    }

    private bool AnalyzeClassDeclaration(ClassDeclaration node)
    {
        if (_context!.CheckWord(node.Name.Value))
        {
            LogNameAlreadyUsedError(node.Name);
            return false;
        }
        _context.ReserveWord(node.Name.Value);

        _context = new Context(_context);
        bool ok = true;
        foreach (var method in node.Functions)
        {
            ok &= Analyze(method);
        }

        _context = _context.OuterContext!;
        return ok;
    }

    private bool AnalyzeVariableDeclaration(VariableDeclaration node)
    {
        if (!_context.CheckWord(node.Name.Value))
        {
            _context.SetWordType(node.Name.Value, node.Type.Name.Value);
            return true;
        }
        LogNameAlreadyUsedError(node.Name);
        return false;
    }

    private void LogNameAlreadyUsedError(Token token)
    {
        LogError($"Name {token} is already used", token);
    }

    private void LogUnknownNameError(Token token)
    {
        LogError($"Unknown name {token}", token);
    }

    private void LogIncompatibleTypes(string left, string right, Token where)
    {
        LogError($"Incompatible types in expression: {left} and {right}", where);
    }

    private void LogIncompatibleConversionTypes(string left, string right, Token where)
    {
        LogError($"Can't convert type {right} to type {left}", where);
    }

    private void LogError(string message, Token token)
    {
        _logger.WriteLine($"[Semantic Analyzer] Error at line {token.LineNumber} and column {token.ColumnNumber}: {message}");
    }

    private bool TryGetExpressionType(Expression left, Expression right, out string type)
    {
        type = "";
        if (!TryGetExpressionType(left, out var leftType) ||
            !TryGetExpressionType(right, out var rightType))
            return false;
        
        if(!TryGetExpressionType(leftType, rightType, out type))
        {
            LogIncompatibleTypes(leftType, rightType, GetExpressionLeftToken(left));
            return false;
        }

        return true;
    }

    private bool TryGetExpressionType(Expression expression, out string leftType)
    {
        leftType = "";
        switch (expression)
        {
            case BinaryExpression leftExp:
            {
                return TryGetExpressionType(leftExp.Left, leftExp.Right, out leftType);
            }
            case UnaryExpression unaryLeftExp:
            {
                var name = unaryLeftExp.Value.Value.Value;
                if (unaryLeftExp.Value.Value.Type != TokenType.Value && !_context.WordHasType(name))
                {
                    LogUnknownNameError(unaryLeftExp.Value.Value);
                    return false;
                }

                leftType = unaryLeftExp.Value.Value.Type == TokenType.Value 
                    ? _context.GetValueType(name) 
                    : _context.GetWordType(name);
                break;
            }
            case ValueExpression valueExpression:
            {
                var name = valueExpression.Value.Value.Value;
                if (valueExpression.Value.Value.Type != TokenType.Value && !_context.WordHasType(name))
                {
                    LogUnknownNameError(valueExpression.Value.Value);
                    return false;
                }

                leftType = valueExpression.Value.Value.Type == TokenType.Value 
                    ? _context.GetValueType(name) 
                    : _context.GetWordType(name);
                break;
            }
        }

        return true;
    }

    private Token GetExpressionLeftToken(Expression expression)
    {
        return expression switch
        {
            BinaryExpression binaryExpression => GetExpressionLeftToken(binaryExpression.Left),
            UnaryExpression unaryExpression => unaryExpression.Value.Value,
            ValueExpression valueExpression => valueExpression.Value.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(expression))
        };
    }
    
    private bool TryGetExpressionType(string left, string right, out string type)
    {
        type = "";
        
        if (CanBeConverted(left, right))
        {
            type = left;
            return true;
        }
        if (CanBeConverted(right, left))
        {
            type = right;
            return true;
        }
        
        return false;
    }

    private bool CanBeConverted(string from, string to)
    {
        if (from == to) return true;
        
        return to switch
        {
            "int" => from is "float" or "double" or "char",
            "float" => from is "double" or "int" or "char", 
            "double" => from is "float" or "int" or "char",
            "char" => from is "int",
            "string" => from is "char",
            _ => false
        };
    }
}