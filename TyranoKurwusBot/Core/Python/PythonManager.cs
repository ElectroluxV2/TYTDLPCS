namespace TyranoKurwusBot.Core.Python;

public static partial class PythonManager
{
    public static readonly string PythonPath = "python3";
    public static readonly string PipPath = "pip3";


    public static void EnsurePackageExists(string packageName)
    {
        var process = CreateCliProcess("pip3", $"show {packageName}");
        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new FileNotFoundException("Python package not found", packageName);
        }
    }
}