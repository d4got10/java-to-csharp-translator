using CodeGeneration;
using LexicalAnalysis;
using Microsoft.AspNetCore.Mvc;
using SemanticAnalysis;
using Shared.Logs;
using SyntaxAnalysis;

namespace Web.Controllers;

public class HomeController : Controller
{
    public HomeController(SyntaxAnalyzer syntaxAnalyzer, LazyLogger logger)
    {
        _syntaxAnalyzer = syntaxAnalyzer;
        _logger = logger;
    }

    private readonly SyntaxAnalyzer _syntaxAnalyzer;
    private readonly LazyLogger _logger;

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

        try
        {
            if (!_syntaxAnalyzer.Parse(lexemes, semanticMessenger))
            {
                return "Андрей тут ашибка\n" + _logger.GetLogs();
            }
        }
        catch (Exception exception)
        {
            return "Андрей тут ашибка в syntax analyzer\n" + exception;
        }

        try
        {
            var semanticAnalyzer = new SemanticAnalyzer(_logger);
            if (!semanticAnalyzer.Analyze(semanticMessenger.Root))
            {
                return "Андрей я не панимаю\n" + _logger.GetLogs();
            }
        }
        catch (Exception exception)
        {
            return "Андрей тут ашибка в semantic analyzer\n" + exception;
        }

        string output;
        
        try
        {
            var generator = new CodeGenerator();
            output = generator.Generate(semanticMessenger.Root);
        }
        catch (Exception exception)
        {
            return "Андрей тут ашибка в code generator\n" + exception;
        }
        
        return output;
    }
}