using LexicalAnalysis;

const string path = "input.txt";

if (!File.Exists(path))
{
    Console.WriteLine("File not found");
    Console.WriteLine("Files present in the directory: ");
    foreach(var fileName in Directory.GetFiles(AppContext.BaseDirectory))
        Console.WriteLine(fileName);
    return;
}

var lexemes = Lexer.Parse(ReadFile(path));
foreach(var lexeme in lexemes)
    Console.WriteLine(lexeme);


static string ReadFile(string path) => File.ReadAllText(path);