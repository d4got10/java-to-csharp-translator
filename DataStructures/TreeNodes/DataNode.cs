namespace DataStructures.TreeNodes;

public class DataNode : Node
{
    public string Value;

    public override string ToString()
    {
        return $"Data node: {Value}";
    }
}