using System.Diagnostics;

namespace TYTDLPCS;

public static partial class ScrapManager
{
    private static Process CreateCliProcess(string executable, string arguments)
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