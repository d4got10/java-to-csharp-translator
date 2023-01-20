using CodeGeneration;
using LexicalAnalysis;
using SemanticAnalysis;
using Shared.Logs;
using SyntaxAnalysis;

namespace TranslatorTests;

public class Tests
{
    private const string PathToSolutionDirectory = "../../../../";
    private const string PathToTestData = "../../../Data/";
    
    private Lexer _lexer;
    private SyntaxAnalyzer _syntaxAnalyzer;
    private SemanticAnalyzer _semanticAnalyzer;
    private CodeGenerator _codeGenerator;
    private LazyLogger _logger;
    
    [SetUp]
    public void Setup()
    {
        _logger = new LazyLogger();
        _lexer = new Lexer();
        _syntaxAnalyzer = new SyntaxAnalyzer(
            PathToSolutionDirectory + "Grammar.json", 
            PathToSolutionDirectory + "SyntaxErrors.json", 
            _logger);
        _semanticAnalyzer = new SemanticAnalyzer(_logger);
        _codeGenerator = new CodeGenerator();
    }

    [Test]
    public void TestDeclaration() => TestWithFiles("Declaration.txt", "Declaration.txt");
    [Test]
    public void TestWhile() => TestWithFiles("While.txt", "While.txt");
    [Test]
    public void TestFor() => TestWithFiles("For.txt", "For.txt");

    private void TestWithFiles(string inputFileName, string outputFileName)
    {
        var lexems = _lexer.Parse(GetTestInput(inputFileName));
        var messenger = new SemanticMessenger();
        if (!_syntaxAnalyzer.Parse(lexems, messenger)) 
            Assert.Fail($"Syntax analyzer failed\n{_logger.GetLogs()}");
        if(!_semanticAnalyzer.Analyze(messenger.Root))
            Assert.Fail($"Semantic analyzer failed\n{_logger.GetLogs()}");
        var result = _codeGenerator.Generate(messenger.Root);
        result = result.Replace("\t", "    ");

        var expected = GetTestOutput(outputFileName);
        expected = expected.Replace("\r\n", "\n");
        
        if (result != expected)
        {
            int firstOccurrence = result.Zip(expected, (p, q) => new { A = p, B = q })
                .Select((p, i) => new { A = p.A, B = p.B, idx = i })
                .Where(p => p.A != p.B)
                .Select(p => p.idx)
                .First();
            string temp = result[..firstOccurrence] + "$" + result[firstOccurrence..];
            Assert.Fail(
                $"Translator output doesn't match expected output.\nMismatch:\n{temp}\nResult:\n{result}\nExpected:\n{expected}\n");
        }

        Assert.Pass();
    }

    private static string GetTestInput(string fileName)
    {
        var path = AppContext.BaseDirectory + PathToTestData + "Input/" + fileName;
        return ReadFile(path);
    }

    private static string GetTestOutput(string fileName)
    {
        var path = AppContext.BaseDirectory + PathToTestData + "Output/" + fileName;
        return ReadFile(path);
    }
    
    private static Dictionary<string, string> GetTestFilesFromFolder(string folder)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var fileName in Directory.GetFiles(AppContext.BaseDirectory+"../../../unit_test/"+folder))
        {
            string[] splits;
            splits = fileName.Split("/");
            dictionary.Add(splits[^1].Split(".")[0], ReadFile(fileName));
        }
        return dictionary;
    }
    private static string ReadFile(string path) => File.ReadAllText(path);
}