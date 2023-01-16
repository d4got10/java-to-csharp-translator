namespace Shared.TreeNodes;

public class TypeNode : Node
{
    public Token Name;
    public bool IsArrayType;
    
    public override string ToString()
    {
        return $"Type node: {Name} {(IsArrayType ? "[]" : "")}";
    }
}