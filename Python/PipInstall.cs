namespace TYTDLPCS.Python;

public static partial class PythonManager
{
    public static bool PipInstall(string package) {
        var pipinstall = RunPipCommand($"install --upgrade-strategy eager {package}");

        return pipinstall.ExitCode == 0;
    }
}