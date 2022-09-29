namespace TYTDLPCS;

public static partial class ScrapManager
{
    public static readonly string PythonPath = ExamineWithWhich("python3");
    public static readonly string PipPath = ExamineWithWhich("pip");

    static ScrapManager()
    {
        var pipInstall = CreateCliProcess(PipPath, "install --upgrade-strategy eager youtube-dl");
        pipInstall.Start();
        var output = pipInstall.StandardOutput.ReadToEnd();
        Console.WriteLine(output);
        pipInstall.WaitForExit();
    }
}