namespace TyranoKurwusBot.Core.Python;

public static partial class PythonManager
{
    private static string ExamineWithWhich(string arguments)
    {
        var process = CreateCliProcess("which", arguments);
        process.Start();
        var path = process.StandardOutput.ReadLine();
        process.WaitForExit();
        return path ?? throw new ApplicationException($"Can't find {arguments} in path");
    }
}