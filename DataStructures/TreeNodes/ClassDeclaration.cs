namespace DataStructures.TreeNodes;

public class ClassDeclaration : Node
{
    public Token Name;
    public Token AccessModifier;
    public List<FunctionDeclaration> Functions = new();
    
    public override string ToString()
    {
        return $"Class node: {AccessModifier} class {Name} ({GetString(Functions)}))";
    }
}