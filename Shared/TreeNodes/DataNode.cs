namespace Shared.TreeNodes;

public class DataNode : Node
{
    public Token Value;

    public override string ToString()
    {
        return $"Data node: {Value}";
    }
}