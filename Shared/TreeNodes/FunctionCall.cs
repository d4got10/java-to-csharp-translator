namespace Shared.TreeNodes;

public class FunctionCall : Node
{
    public Arguments Arguments;
    public DataNode Name;

    public override string ToString()
    {
        return $"Function call node: ({Name})({GetString(Arguments.Expressions)})";
    }
}