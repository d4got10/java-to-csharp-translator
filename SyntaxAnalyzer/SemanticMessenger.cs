using DataStructures;
using DataStructures.TreeNodes;

namespace SyntaxAnalysis;

public class SemanticMessenger
{
    public Node Root => _stack.Peek();
    
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
                    Value = tokens[index]
                });
                break;
            case "#type#":
                _stack.Push(new TypeNode
                {
                    Name = ((DataNode)_stack.Pop()).Value
                });
                break;
            case "#initialization#":
                var initializationValue = (Expression)_stack.Pop();
                var declaration = (VariableDeclaration)_stack.Pop();
                _stack.Push(new InitializedVariableDeclaration
                {
                    Type = declaration.Type,
                    Name = declaration.Name,
                    Value = initializationValue
                });
                break;
            case "#args_start#":
                _stack.Push(new ArgumentsStart());
                break;
            case "#args_end#":
                var arguments = new List<Expression>();
                while (_stack.TryPeek(out var top) && top is not ArgumentsStart) 
                    arguments.Add((Expression)_stack.Pop());
                _stack.Pop();
                _stack.Push(new Arguments
                {
                    Expressions = arguments
                });
                break;
            case "#value_expression#":
                _stack.Pop();
                _stack.Push(new ValueExpression());
                break;
            case "#unary_expression#":
                _stack.Pop();
                _stack.Pop();
                _stack.Push(new UnaryExpression());
                break;
            case "#binary_expression#":
                _stack.Pop();
                _stack.Pop();
                _stack.Pop();
                _stack.Push(new BinaryExpression());
                break;
            case "#assign#":
                _stack.Push(new Assignment
                {
                    Value = (Expression)_stack.Pop(),
                    VariableName = ((DataNode)_stack.Pop()).Value,
                });
                break;
            case "#static#":
                _stack.Push(new Static());
                break;
            case "#array_type#":
                ((TypeNode)_stack.Peek()).IsArrayType = true;
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
                var parameters = new List<VariableDeclaration>();
                while (_stack.TryPeek(out var top) && top is not ParametersStart) 
                    parameters.Add((VariableDeclaration)_stack.Pop());
                _stack.Pop();
                _stack.Push(new Parameters
                {
                    Variables = parameters
                });
                break;
            case "#dot#":
                //TODO: object.object
                break;
            case "#function_call#":
                //TODO: call on object
                _stack.Pop();
                break;
            case "#function_declaration#":
                var instructions = new List<Instruction>();
                
                while (_stack.TryPeek(out var top) && top is Instruction) 
                    instructions.Insert(0, (Instruction)_stack.Pop());
                
                var functionParameters = (Parameters)_stack.Pop();
                var functionData = (VariableDeclaration)_stack.Pop();
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
                _stack.Push(new VariableDeclaration
                {
                    Name = ((DataNode)_stack.Pop()).Value,
                    Type = (TypeNode)_stack.Pop(),
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
            case "#for#":
                var forInstructions = new List<Instruction>();
                while (_stack.Peek() is Instruction)
                    forInstructions.Insert(0, (Instruction)_stack.Pop());
                
                _stack.Push(new For
                {
                    Instructions = forInstructions,
                    Expression = (Assignment)_stack.Pop(),
                    Comparison = (Comparison)_stack.Pop(),
                    Iterator = (VariableDeclaration)_stack.Pop(),
                });
                break;
            case "#while#":
                var whileInstructions = new List<Instruction>();
                while (_stack.Peek() is Instruction)
                    whileInstructions.Insert(0, (Instruction)_stack.Pop());
                
                _stack.Push(new While
                {
                    Instructions = whileInstructions,
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