using System.Text;
using DataStructures.TreeNodes;

namespace CodeGeneration;

public class CodeGenerator
{
    public string Generate(Node root)
    {
        return root switch
        {
            ClassDeclaration classDeclaration => GenerateClassDeclaration(classDeclaration),
            _ => root.GetType().ToString()!
        };
    }

    private string GenerateClassDeclaration(ClassDeclaration node)
    {
        var builder = new StringBuilder();
        builder.Append($"{node.AccessModifier.Value} class {node.Name.Value}\n");
        builder.Append("{\n");
        foreach (FunctionDeclaration functionDeclaration in node.Functions)
        {
            builder.Append(Generate(functionDeclaration) + "\n");
        }
        builder.Append("}\n");
        return builder.ToString();
    }
}