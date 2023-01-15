using CodeGeneration;
using LexicalAnalysis;
using Microsoft.AspNetCore.Mvc;
using SemanticAnalysis;
using SyntaxAnalysis;

namespace Web.Controllers;

public class HomeController : Controller
{
    public HomeController(SyntaxAnalyzer syntaxAnalyzer)
    {
        _syntaxAnalyzer = syntaxAnalyzer;
    }

    private readonly SyntaxAnalyzer _syntaxAnalyzer;

    public IActionResult Index()
    {
        ViewBag.Message = "Программа будет тут";
        return View();
    }
    
    [HttpPost]
    public IActionResult Index([FromForm] string? text)
    {
        ViewBag.Prev = text;
        if (!string.IsNullOrEmpty(text))
            ViewBag.Message = Parse(text);
        else
            ViewBag.Message = "Программа будет тут";
        return View();
    }

    private string Parse(string text)
    {
        var lexer = new Lexer();
        var lexemes = lexer.Parse(text);

        var semanticMessenger = new SemanticMessenger();

        if (!_syntaxAnalyzer.Parse(lexemes, semanticMessenger))
        {
            return "Андрей тут ашибка";
        }

        var semanticAnalyzer = new SemanticAnalyzer();
        if (!semanticAnalyzer.Analyze(semanticMessenger.Root))
        {
            return "Андрей я не панимаю";
        }

        var generator = new CodeGenerator();
        var output = generator.Generate(semanticMessenger.Root);

        return output;
    }
}