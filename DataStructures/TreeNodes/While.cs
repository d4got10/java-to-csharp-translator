namespace DataStructures.TreeNodes;

public class While : Node
{
    public Comparison Comparison;
    public Node Statement;
    
    public override string ToString()
    {
        return $"While node: while ({Comparison}) {{ ({Statement} }}";
    }
}