namespace TYTDLPCS.Python;

public static partial class PythonManager
{
    public static readonly string PackagesPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!, "pip-packages");
    public static readonly string PythonPath = ExamineWithWhich("python3");
    public static readonly string PipPath = ExamineWithWhich("pip3");

    public static string GetPackageBinary(string packageName) => Path.Combine(PackagesPath, "bin", packageName);
}