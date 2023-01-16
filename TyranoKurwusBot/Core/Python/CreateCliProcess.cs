using System.Diagnostics;

namespace TyranoKurwusBot.Core.Python;

public static partial class PythonManager
{
    public static Process CreateCliProcess(string executable, string arguments)
    {
        return new Process
        {
            StartInfo =
            {
                FileName = executable,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };
    }
}