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
            "System.out.println"
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
            Assignment assignment => AnalyzeAssignment(assignment),
            Expression expression => AnalyzeExpression(expression),
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
            return true;
        }
        LogNameAlreadyUsedError(node.Name);
        return false;
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

    private bool AnalyzeAssignment(Assignment node)
    {
        return _context.CheckWord(node.VariableName.Value);
    }

    private bool AnalyzeComparison(Comparison node)
    {
        bool ok = true;
        // if (node.Left is Expression left && node.Right is Expression right)
        // {
        //     if (!_context.WordHasType(left.Value.Value))
        //     {
        //         LogUnknownNameError(left.Value);
        //         ok = false;
        //     }
        //
        //     if (!_context.WordHasType(right.Value.Value))
        //     {
        //         LogUnknownNameError(right.Value);
        //         ok = false;
        //     }
        //
        //     if (!ok) return false;
        //     
        //     var leftType = _context.GetWordType(left.Value.Value);
        //     var rightType = _context.GetWordType(right.Value.Value);
        //     if (leftType != rightType)
        //     {
        //         LogError($"Can't compare type {leftType} to type {rightType}", left.Value);
        //         ok = false;
        //     }
        // }
        ok &= Analyze(node.Left);
        ok &= Analyze(node.Right);

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

    private void LogError(string message, Token token)
    {
        _logger.WriteLine($"[Semantic Analyzer] Error at line {token.LineNumber} and column {token.ColumnNumber}: {message}");
    }
}