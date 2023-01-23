using LexicalAnalysis;

namespace Tests;

public class BaseTest
{

    //Expected и Input для одного и того же теста должны называться одинакого (без учёта расшbрения).
    protected void Test(String projectName)
    {
        var expects = GetTestFilesFromFolder(projectName+"/Expected");
        var inputs = GetTestFilesFromFolder(projectName+"/Input");
        Console.WriteLine($"Lexer.Tests names:");
        foreach (var input in inputs)
        {
            if (expects.ContainsKey(input.Key))
            {
                Console.WriteLine(input.Key);
                Assert.That(GetActual(input.Value), Is.EqualTo(expects[input.Key]));
            }
        }
    }
    
    //Переопределять для каждого модуля
    protected virtual string GetActual(String input)
    {
        return "fet";
    }
    
    private Dictionary<String, String> GetTestFilesFromFolder(String folder)
    {
        Dictionary<String, String> dictionary = new Dictionary<String, String>();
        foreach (var fileName in Directory.GetFiles(AppContext.BaseDirectory+"../../../"+folder))
        {
            string[] splits;
            var newFileName = fileName.Replace("\\", "/");
            splits = newFileName.Split("/");
            dictionary.Add(splits[splits.Length-1].Split(".")[0], ReadFile(fileName));
        }
        return dictionary;
    }
    
    static string ReadFile(string path) => File.ReadAllText(path).ReplaceLineEndings("\n");
}