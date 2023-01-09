namespace DataStructures.TreeNodes;

public class Comparison : Node
{
    public Node Left;
    public Node Operator;
    public Node Right;
    
    public override string ToString()
    {
        return $"Comparison node: ({Left}) ({Operator}) ({Right})";
    }
}