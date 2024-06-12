namespace BibleTextScraper;
public class ErrorLogger
{
    private readonly string logFilePath;

    public ErrorLogger(string logFilePath)
    {
        this.logFilePath = logFilePath;
    }

    public void LogError(Exception exception)
    {
        string errorMessage = 
@$"
{DateTime.Now}
Error: {exception.Message}
Stacktrace: {exception.StackTrace}
Inner exception: {exception.InnerException}
Message: {exception.Message}
Help link: {exception.HelpLink}
Source: {exception.Source}
Target site: {exception.TargetSite}
Data: {exception.Data}
HResult: {exception.HResult}
Runtime type name: {exception.GetType().Name}
Runtime type: {exception.GetType()}
";

        Console.WriteLine($"Error: {exception.Message}");

        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            writer.WriteLine(errorMessage);
        }
    }
}
