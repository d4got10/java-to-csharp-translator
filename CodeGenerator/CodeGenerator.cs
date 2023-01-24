using System.Text;
using Shared.TreeNodes;

namespace CodeGeneration;

public class CodeGenerator
{
    private readonly Dictionary<string, string> _equalityTable = new()
    {
        {"System.out.println", "Console.WriteLine"},
        {"System.out.print", "Console.Write"}
    };

    private readonly HashSet<Type> _dontNeedSemicolon = new()
    {
        typeof(For),
        typeof(While),
        typeof(If),
        typeof(ElseIf),
        typeof(Else)
    };

    private int _tabs = 0;
    
    public string Generate(Node root, int tabCount = 0)
    {
        return root switch
        {
            TypeNode typeNode => GenerateTypeNode(typeNode),
            ValueExpression valueExpression => GenerateValueExpression(valueExpression, tabCount),
            ClassDeclaration classDeclaration => GenerateClassDeclaration(classDeclaration, tabCount),
            FunctionDeclaration functionDeclaration => GenerateFunctionDeclaration(functionDeclaration, tabCount),
            InitializedVariableDeclaration initializedVariableDeclaration => GenerateInitializedVariableDeclaration(initializedVariableDeclaration, tabCount),
            VariableDeclaration variableDeclaration => GenerateVariableDeclaration(variableDeclaration, tabCount),
            Instruction instruction => GenerateInstruction(instruction, tabCount),
            FunctionCall functionCall => GenerateFunctionCall(functionCall, tabCount),
            Arguments arguments => GenerateArguments(arguments, tabCount),
            For @for => GenerateFor(@for, tabCount),
            Comparison comparison => GenerateComparison(comparison),
            BinaryExpression binaryExpression => GenerateBinaryExpression(binaryExpression),
            UnaryExpression unaryExpression => GenerateUnaryExpression(unaryExpression),
            While @while => GenerateWhile(@while, tabCount),
            UnaryAssignment unaryAssignment => GenerateUnaryAssigment(unaryAssignment),
            ExpressionAssignment expressionAssignment => GenerateExpressionAssigment(expressionAssignment),
            DataNode dataNode => dataNode.Value.Value,
            If @if => GenerateIf(@if, tabCount),
            ElseIf elseIf => GenerateElseIf(elseIf, tabCount),
            Else @else => GenerateElse(@else,tabCount),
            DoWhile doWhile => GenerateDoWhile(doWhile, tabCount),
            _ => AddTabs(tabCount) + root.GetType().ToString()!
        };
    }
    
    private string GenerateDoWhile(DoWhile node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append("do\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (var instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}\n");
        builder.Append(AddTabs(tabCount) + $"while({Generate(node.Comparison)})");
        return builder.ToString();
    }
    
    private string GenerateIf(If node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append($"if({Generate(node.Comparison)})\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (var instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        if (node.Tail is not NoTail)
            builder.Append("\n" + AddTabs(tabCount) + Generate(node.Tail, tabCount));
        return builder.ToString();
    }
    
    private string GenerateElseIf(ElseIf node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append($"else if({Generate(node.Comparison)})\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (var instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        if (node.Tail is not NoTail)
            builder.Append("\n" + AddTabs(tabCount) + Generate(node.Tail, tabCount));
        return builder.ToString();
    }
    
    private string GenerateElse(Else node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append("else\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (var instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        return builder.ToString();
    }
    
    private string GenerateExpressionAssigment(ExpressionAssignment node)
    {
        return $"{node.VariableName} = {Generate(node.Value)}";
    }
    
    private string GenerateUnaryAssigment(UnaryAssignment node)
    {
        return $"{node.VariableName}{node.Operator}";
    }
        
    private string GenerateWhile(While node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append($"while({Generate(node.Comparison)})\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (var instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        return builder.ToString();
    }
    private string GenerateBinaryExpression(BinaryExpression node)
    {
        return $"{Generate(node.Left)} {node.Operator.Value.Value} {Generate(node.Right)}";
    }

    private string GenerateUnaryExpression(UnaryExpression node)
    {
        return $"{Generate(node.Value)}{Generate(node.Operator)}";
    }
    
    private string GenerateComparison(Comparison node)
    {
        return GenerateBinaryExpression(node);
    }

    private string GenerateArguments(Arguments node, int tabCount)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < node.Expressions.Count - 1; index++)
        {
            var argument = node.Expressions[index];
            builder.Append(Generate(argument) + ", ");
        }
        builder.Append(Generate(node.Expressions[^1]));
        return builder.ToString();
    }

    private string GenerateFunctionCall(FunctionCall node, int tabCount)
    {
        return $"{_equalityTable[node.Name.Value.Value]}({Generate(node.Arguments)})";
    }

    private string GenerateValueExpression(ValueExpression node, int tabCount)
    {
        return node.Value.Value.Value;
    }

    private string GenerateTypeNode(TypeNode node)
    {
        return $"{node.Name.Value}{(node.IsArrayType ? "[]" : "")}";
    }

    private string GenerateInitializedVariableDeclaration(InitializedVariableDeclaration node, int tabCount)
    {
        return $"{Generate(node.Type)} {node.Name} = {Generate(node.Value)}";
    }

    private string GenerateFor(For node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append($"for ({Generate(node.Iterator)}; {Generate(node.Comparison)}; {Generate(node.Expression)})\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (var instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        
        return builder.ToString();
    }

    private string GenerateInstruction(Instruction node, int tabCount)
    {
        bool needSemicolon = !_dontNeedSemicolon.Contains(node.Inner.GetType());
        
        return AddTabs(tabCount) + Generate(node.Inner, tabCount) + (needSemicolon ? ";" : "");
    }

    private string GenerateVariableDeclaration(VariableDeclaration variableDeclaration, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append($"{Generate(variableDeclaration.Type)} {variableDeclaration.Name.Value}");
        return builder.ToString();
    }

    private string GenerateFunctionDeclaration(FunctionDeclaration node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append(AddTabs(tabCount) + $"{node.AccessModifier.Value} {(node.IsStatic ? "static" : "")} {Generate(node.Data, tabCount + 1)}(");
        for (int i = 0; i < node.Parameters.Variables.Count - 1; i++)
        {
            builder.Append(Generate(node.Parameters.Variables[i], tabCount + 1) + ", ");
        }

        builder.Append(Generate(node.Parameters.Variables[^1], tabCount + 1) + ")\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (Instruction instruction in node.Instructions)
        {
            builder.Append(Generate(instruction, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        return builder.ToString();
    }

    private string GenerateClassDeclaration(ClassDeclaration node, int tabCount)
    {
        var builder = new StringBuilder();
        builder.Append($"{node.AccessModifier.Value} class {node.Name.Value}\n");
        builder.Append(AddTabs(tabCount) + "{\n");
        foreach (FunctionDeclaration functionDeclaration in node.Functions)
        {
            builder.Append(Generate(functionDeclaration, tabCount + 1) + "\n");
        }
        builder.Append(AddTabs(tabCount) + "}");
        return builder.ToString();
    }

    private string AddTabs(int count)
    {
        StringBuilder tabs = new();
        for (int i = 0; i < count; i++)
        {
            tabs.Append('\t');
        }
        return tabs.ToString();
    }
}