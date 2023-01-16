namespace Shared.TreeNodes;

public class FunctionDeclaration : Node
{
    public VariableDeclaration Data;
    public Token AccessModifier;
    public bool IsStatic;
    public List<Instruction> Instructions = new();
    public Parameters Parameters;
    
    public override string ToString()
    {
        return $"Function node: {AccessModifier} {(IsStatic ? "static" : "")} ({Data}) ({Parameters}) ({GetString(Instructions)})";
    }
}