namespace DataStructures.TreeNodes;

public class Parameters : Node
{
    public List<Variable> Variables = new();
    
    public override string ToString()
    {
        return $"Parameters node: ({GetString(Variables)}))";
    }
}