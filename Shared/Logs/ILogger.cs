namespace Shared.Logs;

public interface ILogger
{
    void Write(string text);
    void WriteLine(string text);
}