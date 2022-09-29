namespace TYTDLPCS.Python;

public static partial class PythonManager
{
    public static readonly string PythonPath = ExamineWithWhich("python3");
    public static readonly string PipPath = ExamineWithWhich("pip");

    static PythonManager()
    {
        // var pipInstall = CreateCliProcess(PipPath, "install --upgrade-strategy eager youtube-dl");
        // pipInstall.Start();
        // var output = pipInstall.StandardOutput.ReadToEnd();
        // Console.WriteLine(output);
        // pipInstall.WaitForExit();
    }
}