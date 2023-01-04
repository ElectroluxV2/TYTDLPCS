using CliWrap;

namespace TYTDLPCS.Python;

public partial class PythonManager
{
    public static Command InstalPackage(string PackageName)
    {
        return Cli
            .Wrap(PipPath)
            .WithArguments($"install --target {PackagesPath} --upgrade-strategy eager --upgrade {PackageName}");
    }
}