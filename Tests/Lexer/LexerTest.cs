using NUnit.Framework.Internal;

namespace Tests.Lexer;

public class LexerTest: BaseTest
{
    protected override string GetActual(String input)
    {
        string actual = "";
        var lexemes = new LexicalAnalysis.Lexer().Parse(input);
        foreach(var lexeme in lexemes)
            actual = actual + lexeme.Type+" " +lexeme.Value + " " + lexeme.LineNumber +":"+lexeme.ColumnNumber + "\n";
        return actual;
    }

    [Test]
    public void LexerUnitTest()
    {
        this.Test("Lexer");
    }
}