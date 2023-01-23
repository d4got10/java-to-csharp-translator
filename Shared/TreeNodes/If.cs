namespace Shared.TreeNodes;

public class If : Node
{
    public List<Instruction> Instructions;
    public Comparison Comparison;
    public IfTail Tail = new NoTail();
}

public class IfTail : Node
{
}

public class NoTail : IfTail
{
}

public class ElseIf : IfTail
{
    public List<Instruction> Instructions;
    public Comparison Comparison;
    public IfTail Tail;
}

public class Else : IfTail
{
    public List<Instruction> Instructions;
}