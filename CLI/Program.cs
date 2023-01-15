using CodeGeneration;
using LexicalAnalysis;
using SemanticAnalysis;
using SyntaxAnalysis;

string path = args.Length > 0 ? args[0] : throw new Exception("Не указан путь к файлу с исходным кодом");
string grammarPath = args.Length > 1 ? args[1] : throw new Exception("Не указан путь к граматике");
string syntaxErrorsPath = args.Length > 2 ? args[2] : throw new Exception("Не указан путь к ошибкам синтаксиса");

if (!File.Exists(path))
{
    Console.WriteLine("File not found");
    Console.WriteLine("Files present in the directory: ");
    foreach(var fileName in Directory.GetFiles(AppContext.BaseDirectory))
        Console.WriteLine(fileName);
    return;
}

var lexer = new Lexer();
var lexemes = lexer.Parse(ReadFile(path));
// foreach(var lexeme in lexemes)
//     Console.WriteLine(lexeme.Type+" " +lexeme.Value + " " + lexeme.LineNumber +":"+lexeme.ColumnNumber);

var semanticMessenger = new SemanticMessenger();

var syntax = new SyntaxAnalyzer(grammarPath, syntaxErrorsPath);
if (!syntax.Parse(lexemes, semanticMessenger))
{
    Console.WriteLine("Андрей тут ашибка");
    return;
}

var semanticAnalyzer = new SemanticAnalyzer();
if (!semanticAnalyzer.Analyze(semanticMessenger.Root))
{
    Console.WriteLine("Андрей я не панимаю");
    return;
}

Console.WriteLine("Андрей спасибо всё харошо");

var generator = new CodeGenerator();
var output = generator.Generate(semanticMessenger.Root);

Console.WriteLine("Программа: \n" + output);

static string ReadFile(string path) => File.ReadAllText(path);