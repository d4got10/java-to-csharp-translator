namespace DataStructures.TreeNodes;

public abstract class Node
{
    protected static string GetString(IEnumerable<object> enumerable)
    {
        return enumerable.Select(t => $" ({t}) ").Aggregate((x, t) => x + t);
    }
}