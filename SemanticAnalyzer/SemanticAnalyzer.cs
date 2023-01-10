using DataStructures;
using DataStructures.TreeNodes;

namespace SemanticAnalysis;

public class SemanticAnalyzer
{
    private readonly HashSet<string> _usedWords = new();
    private readonly Dictionary<string, string> _definedWordTypes = new();

    private Context _context = new();
    
    public bool Analyze(Node root)
    {
        return root switch
        {
            DataNode dataNode => AnalyzeDataNode(dataNode),
            ClassDeclaration classDeclaration => AnalyzeClassDeclaration(classDeclaration),
            FunctionDeclaration functionDeclaration => AnalyzeFunctionDeclaration(functionDeclaration),
            InitializedVariableDeclaration initializedVariableDeclaration => AnalyzeInitializedVariableDeclaration(initializedVariableDeclaration),
            VariableDeclaration variable => AnalyzeVariableDeclaration(variable),
            Instruction instruction => AnalyzeInstruction(instruction),
            While @while => AnalyzeWhile(@while),
            For @for => AnalyzeFor(@for),
            Comparison comparison => AnalyzeComparison(comparison),
            Assignment assignment => AnalyzeAssignment(assignment),
            _ => throw new Exception("Unknown node type: " + root.GetType())
        };
    }

    private bool AnalyzeInitializedVariableDeclaration(InitializedVariableDeclaration node)
    {
        if (!_context.CheckWord(node.Name.Value))
        {
            _context.SetWordType(node.Name.Value, node.Type.Name.Value);
            return true;
        }
        LogError($"Name {node.Name} is already used", node.Name);
        return false;
    }

    private bool AnalyzeDataNode(DataNode node)
    {
        if (!_context.CheckWord(node.Value.Value))
        {
            LogError($"Unknown name {node.Value}", node.Value);
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
        if (node.Left is DataNode left && node.Right is DataNode right)
        {
            if (!_context.WordHasType(left.Value.Value))
            {
                LogError($"Unknown name {left.Value}", left.Value);
                ok = false;
            }

            if (!_context.WordHasType(right.Value.Value))
            {
                LogError($"Unknown name {right.Value}", right.Value);
                ok = false;
            }

            if (!ok) return false;
            
            var leftType = _context.GetWordType(left.Value.Value);
            var rightType = _context.GetWordType(right.Value.Value);
            if (leftType != rightType)
            {
                LogError($"Can't compare type {leftType} to type {rightType}", left.Value);
                ok = false;
            }
        }

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
            LogError($"Name {node.Name} is already used", node.Name);
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
        LogError($"Name {node.Name} is already used", node.Name);
        return false;
    }

    private void LogError(string message, Token token)
    {
        Console.WriteLine($"[Semantic Analyzer] Error at line {token.LineNumber} and column {token.ColumnNumber}: {message}");
    }
}