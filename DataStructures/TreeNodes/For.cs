namespace DataStructures.TreeNodes;

public class For : Node
{
    public VariableDeclaration Iterator;
    public Comparison Comparison;
    public Node Expression;
    public List<Instruction> Instructions = new();
    
    public override string ToString()
    {
        return $"For node: for ( ({Iterator}) ; ({Comparison}) ; ({Expression}) ) {{ {GetString(Instructions)} }}";
    }
}