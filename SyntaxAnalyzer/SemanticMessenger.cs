using Shared;
using Shared.TreeNodes;

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
                _stack.Push(new ValueExpression
                {
                    Value = (DataNode)_stack.Pop()
                });
                break;
            case "#unary_expression#":
                _stack.Push(new UnaryExpression
                {
                    Operator = (DataNode)_stack.Pop(),
                    Value = (DataNode)_stack.Pop()
                });
                break;
            case "#binary_expression#":
                _stack.Push(new BinaryExpression
                {
                    Right = (Expression)_stack.Pop(),
                    Operator = (DataNode)_stack.Pop(),
                    Left = (Expression)_stack.Pop()
                });
                break;
            case "#unary_assign#":
                var expression = (UnaryExpression)_stack.Pop();
                
                _stack.Push(new UnaryAssignment
                {
                    VariableName = expression.Value.Value,
                    Operator = expression.Operator.Value
                });
                break;
            case "#expression_assign#":
                _stack.Push(new ExpressionAssignment
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
                var popped = ((DataNode)_stack.Pop()).Value;
                var parent = ((DataNode)_stack.Pop()).Value;
                _stack.Push(new DataNode
                {
                    Value = new Token(parent.Type, parent.Value + "." + popped.Value, parent.ColumnNumber, parent.LineNumber)
                });
                break;
            case "#function_call#":
                var callArguments = (Arguments)_stack.Pop();
                var name = (DataNode)_stack.Pop();
                _stack.Push(new FunctionCall
                {
                    Arguments = callArguments,
                    Name = name
                });
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
            case "#do_while#":
                var doWhileComparison = (Comparison)_stack.Pop();
                var doWhileInstructions = new List<Instruction>();
                while (_stack.Peek() is Instruction)
                    doWhileInstructions.Add((Instruction)_stack.Pop());
                
                _stack.Push(new DoWhile
                {
                    Instructions = doWhileInstructions,
                    Comparison = doWhileComparison,
                });
                break;
            case "#condition#":
                break;
            case "#if#":
                var ifInstructions = new List<Instruction>();
                while (_stack.Peek() is Instruction)
                    ifInstructions.Add((Instruction)_stack.Pop());
                
                _stack.Push(new If
                {
                    Instructions = ifInstructions,
                    Comparison = (Comparison)_stack.Pop()
                });
                break;
            case "#else_if#":
                var ifNode = (If)_stack.Pop();
                var elseIfNode = new ElseIf
                {
                    Instructions = ifNode.Instructions,
                    Comparison = ifNode.Comparison,
                    Tail = ifNode.Tail
                };
                
                if(_stack.Peek() is If @if)
                    @if.Tail = elseIfNode;
                else if (_stack.Peek() is ElseIf elseIf)
                    elseIf.Tail = elseIfNode;
                break;
            case "#else#":
                var elseInstructions = new List<Instruction>();
                while (_stack.Peek() is Instruction)
                    elseInstructions.Add((Instruction)_stack.Pop());

                var elseNode = new Else
                {
                    Instructions = elseInstructions
                };
                
                if(_stack.Peek() is If if2)
                    if2.Tail = elseNode;
                else if (_stack.Peek() is ElseIf elseIf)
                    elseIf.Tail = elseNode;
                break;
            case "#comparison#":
                _stack.Push(new Comparison
                {
                    Right = (Expression)_stack.Pop(),
                    Operator = (DataNode)_stack.Pop(),
                    Left = (Expression)_stack.Pop()
                });
                break;
            default:
                Console.WriteLine($"Unhandled message: {value}");
                break;
        }
    }
}