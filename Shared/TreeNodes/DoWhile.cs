namespace Shared.TreeNodes;

public class DoWhile : Node
{
    public Comparison Comparison;
    public List<Instruction> Instructions = new();
    
    public override string ToString()
    {
        return $"DoWhile node: do {{ ({GetString(Instructions)} }} while ({Comparison})";
    }
}