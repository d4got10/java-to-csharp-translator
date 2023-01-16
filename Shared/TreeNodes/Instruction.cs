namespace Shared.TreeNodes;

public class Instruction : Node
{
    public Node Inner;

    public override string ToString()
    {
        return $"Instruction node: ({Inner})";
    }
}