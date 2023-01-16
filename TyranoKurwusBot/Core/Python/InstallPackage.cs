using CliWrap;

namespace TyranoKurwusBot.Core.Python;

public partial class PythonManager
{
    public static Command InstalPackage(string PackageName)
    {
        return Cli
            .Wrap(PipPath)
            .WithArguments($"install --target {PackagesPath} --upgrade-strategy eager --upgrade {PackageName}");
    }
}