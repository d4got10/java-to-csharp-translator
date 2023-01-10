namespace DataStructures.TreeNodes;

public class Arguments : Node
{
    public List<Expression> Expressions = new();
    
    public override string ToString()
    {
        return $"Parameters node: ({GetString(Expressions)}))";
    }
}