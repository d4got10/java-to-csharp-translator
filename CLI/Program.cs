using LexicalAnalysis;
using SemanticAnalysis;
using SyntaxAnalysis;

string path = args.Length > 0 ? args[0] : "input.java";

if (!File.Exists(path))
{
    Console.WriteLine("File not found");
    Console.WriteLine("Files present in the directory: ");
    foreach(var fileName in Directory.GetFiles(AppContext.BaseDirectory))
        Console.WriteLine(fileName);
    return;
}

var lexemes = Lexer.Parse(ReadFile(path));
// foreach(var lexeme in lexemes)
//     Console.WriteLine(lexeme.Type+" " +lexeme.Value + " " + lexeme.LineNumber +":"+lexeme.ColumnNumber);

var semanticMessenger = new SemanticMessenger();

var syntax = new SyntaxAnalyzer();
if (!syntax.Parse(lexemes, semanticMessenger))
{
    Console.WriteLine("Андрей тут ашибка");
    return;
}

//A.frun(... , b.func())

var semanticAnalyzer = new SemanticAnalyzer();
if (!semanticAnalyzer.Analyze(semanticMessenger.Root))
{
    Console.WriteLine("Андрей я не панимаю");
    return;
}

Console.WriteLine("Андрей спасибо всё харошо");

static string ReadFile(string path) => File.ReadAllText(path);