namespace DataStructures.TreeNodes;

public class Assignment : Node
{
    public Variable Variable;
    public string Value;

    public override string ToString()
    {
        return $"Assignment node: ({Variable}) = {Value}";
    }
}