namespace DataStructures.TreeNodes;

public class FunctionDeclaration : Node
{
    public Variable Data;
    public string AccessModifier;
    public bool IsStatic;
    public List<Instruction> Instructions = new();
    public Parameters Parameters;
    
    public override string ToString()
    {
        return $"Function node: {AccessModifier} {(IsStatic ? "static" : "")} ({Data}) ({Parameters}) ({GetString(Instructions)})";
    }
}