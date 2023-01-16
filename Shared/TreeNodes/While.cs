namespace Shared.TreeNodes;

public class While : Node
{
    public Comparison Comparison;
    public List<Instruction> Instructions = new();
    
    public override string ToString()
    {
        return $"While node: while ({Comparison}) {{ ({GetString(Instructions)} }}";
    }
}