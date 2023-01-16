namespace Shared.TreeNodes;

public class InitializedVariableDeclaration : VariableDeclaration
{
    public Expression Value;

    public override string ToString()
    {
        return base.ToString() + $" = ({Value})";
    }
}