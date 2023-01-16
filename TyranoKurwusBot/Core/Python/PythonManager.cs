using System.Reflection;

namespace TyranoKurwusBot.Core.Python;

public static partial class PythonManager
{
    public static readonly string PackagesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "pip-packages");
    public static readonly string PythonPath = ExamineWithWhich("python3");
    public static readonly string PipPath = ExamineWithWhich("pip3");

    public static string GetPackageBinary(string packageName)
    {
        return Path.Combine(PackagesPath, "bin", packageName);
    }

    public static void EnsurePackageExists(string packageName)
    {
        // if (!File.Exists(packageName))
        // {
        //     throw new FileNotFoundException("Python package not found", packageName);
        // }
    }
}