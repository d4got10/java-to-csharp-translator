namespace Shared.TreeNodes;

public class Assignment : Node
{
    public Token VariableName;
}

public class ExpressionAssignment : Assignment
{
    public Expression Value;

    public override string ToString()
    {
        return $"Expression Assignment node: ({VariableName}) = {Value}";
    }
}

public class UnaryAssignment : Assignment
{
    public Token Operator;
}

public class PostfixUnaryAssignment : Node
{
    public Token VariableName;
    public Token Operator;
    
    public override string ToString()
    {
        return $"Assignment node: {VariableName}{Operator}";
    }
}