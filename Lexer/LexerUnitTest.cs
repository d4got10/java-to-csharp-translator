using NUnit.Framework;
using Sandbox;
using LexicalAnalysis;

namespace LexicalAnalysis;

public class LexerUnitTest
{
    [Test]
    public void LexerTests()
    {
        var expecteds = GetTestFilesFromFolder("expected");
        var inputs = GetTestFilesFromFolder("input");
        Console.WriteLine("Tests names:");
        foreach (var input in inputs)
        {
            if (expecteds.ContainsKey(input.Key))
            {
                Console.WriteLine(input.Key);
                Assert.AreEqual(expecteds[input.Key], GetActual(input.Value));
            }
        }
    }


    private string GetActual(String input)
    {
        string actual = "";
        var lexemes = Lexer.Parse(input);
        foreach(var lexeme in lexemes)
            actual = actual + lexeme.Type+" " +lexeme.Value + " " + lexeme.LineNumber +":"+lexeme.ColumnNumber + "\n";
        return actual;
    }
    
    private Dictionary<String, String> GetTestFilesFromFolder(String folder)
    {
        Dictionary<String, String> dictionary = new Dictionary<String, String>();
        foreach (var fileName in Directory.GetFiles(AppContext.BaseDirectory+"../../../unit_test/"+folder))
        {
            string[] splits;
            splits = fileName.Split("/");
            dictionary.Add(splits[splits.Length-1].Split(".")[0], ReadFile(fileName));
        }
        return dictionary;
    }
    static string ReadFile(string path) => File.ReadAllText(path);
}