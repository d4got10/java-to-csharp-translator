using System.Text;

namespace Shared.Logs;

public class LazyLogger : ILogger
{
    private readonly StringBuilder _logs = new();
    
    public string GetLogs() => _logs.ToString();
    public void Write(string text) => _logs.Append(text);
    public void WriteLine(string text) => _logs.AppendLine(text);
}