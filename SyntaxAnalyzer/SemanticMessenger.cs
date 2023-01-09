using DataStructures;
using DataStructures.TreeNodes;

namespace SyntaxAnalysis;

public class SemanticMessenger
{
    private readonly Stack<Node> _stack = new();

    public void DisplayStack()
    {
        foreach (Node node in _stack)
        {
            Console.WriteLine(node);
        }
    }
    
    public void Message(string value, List<Token> tokens, int index)
    {
        switch (value)
        {
            case "#push#":
                _stack.Push(new DataNode
                {
                    Value = tokens[index].Value
                });
                break;
            case "#assign#":
                _stack.Push(new Assignment
                {
                    Value = ((DataNode)_stack.Pop()).Value,
                    Variable = (Variable)_stack.Pop(),
                });
                break;
            case "#static#":
                _stack.Push(new Static());
                break;
            case "#array_type#":
                var typeNode = (DataNode)_stack.Pop();
                _stack.Push(new DataNode
                {
                    Value = typeNode.Value + "[]"
                });
                break;
            case "#class_declaration#":
                var classMethods = new List<FunctionDeclaration>();
                while (_stack.TryPeek(out var top) && top is FunctionDeclaration) 
                    classMethods.Add((FunctionDeclaration)_stack.Pop());
                
                _stack.Push(new ClassDeclaration
                {
                    Name = ((DataNode)_stack.Pop()).Value,
                    AccessModifier = ((DataNode)_stack.Pop()).Value,
                    Functions = classMethods
                });
                break;
            case "#params_start#":
                _stack.Push(new ParametersStart());
                break;
            case "#params_end#":
                var parameters = new List<Variable>();
                while (_stack.TryPeek(out var top) && top is not ParametersStart) 
                    parameters.Add((Variable)_stack.Pop());
                _stack.Pop();
                _stack.Push(new Parameters
                {
                    Variables = parameters
                });
                break;
            case "#function_declaration#":
                var instructions = new List<Instruction>();
                
                while (_stack.TryPeek(out var top) && top is Instruction) 
                    instructions.Add((Instruction)_stack.Pop());
                
                var functionParameters = (Parameters)_stack.Pop();
                var functionData = (Variable)_stack.Pop();
                bool isStatic = _stack.Peek() is Static;
                if (isStatic)
                    _stack.Pop();
                _stack.Push(new FunctionDeclaration
                {
                    Data = functionData,
                    AccessModifier = ((DataNode)_stack.Pop()).Value,
                    IsStatic = isStatic,
                    Instructions = instructions,
                    Parameters = functionParameters
                });
                break;
            case "#declaration#":
                _stack.Push(new Variable
                {
                    Name = ((DataNode)_stack.Pop()).Value,
                    Type = ((DataNode)_stack.Pop()).Value,
                });
                break;
            case "#instruction#":
                _stack.Push(new Instruction
                {
                    Inner = _stack.Pop()
                });
                break;
            case "#statement#":
                break;
            case "#while#":
                _stack.Push(new While
                {
                    Statement = _stack.Pop(),
                    Comparison = (Comparison)_stack.Pop(),
                });
                break;
            case "#condition#":
                break;
            case "#comparison#":
                _stack.Push(new Comparison
                {
                    Right = _stack.Pop(),
                    Operator = _stack.Pop(),
                    Left = _stack.Pop()
                });
                break;
            default:
                Console.WriteLine($"Unhandled message: {value}");
                break;
        }
    }
}