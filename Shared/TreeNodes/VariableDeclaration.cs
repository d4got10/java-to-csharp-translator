namespace Shared.TreeNodes;

public class VariableDeclaration : Node
{
    public TypeNode Type;
    public Token Name;
    
    public override string ToString()
    {
        return $"Variable declaration node: ({Type}) {Name}";
    }
}