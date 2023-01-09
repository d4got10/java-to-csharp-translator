namespace DataStructures.TreeNodes;

public class Variable : Node
{
    public string Type;
    public string Name;

    public override string ToString()
    {
        return $"Variable node: {Type} {Name}";
    }
}