namespace Shared.TreeNodes;

public class Parameters : Node
{
    public List<VariableDeclaration> Variables = new();
    
    public override string ToString()
    {
        return $"Parameters node: ({GetString(Variables)}))";
    }
}