using System.Diagnostics;

namespace TYTDLPCS;

public static partial class ScrapManager
{
    private static string ExaminePythonPath()
    {
        var process = new Process
        {
            StartInfo =
            {
                FileName = "which",
                Arguments = "python3",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        process.Start();
        var path = process.StandardOutput.ReadLine();
        process.WaitForExit();
        return path ?? throw new ApplicationException("Can't find python3 in path");
    }
}