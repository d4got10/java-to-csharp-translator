namespace Shared.TreeNodes;

public class Assignment : Node
{
    public Token VariableName;
    public Expression Value;

    public override string ToString()
    {
        return $"Assignment node: ({VariableName}) = {Value}";
    }
}