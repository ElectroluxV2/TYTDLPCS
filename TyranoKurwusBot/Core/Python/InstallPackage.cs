using CliWrap;

namespace TyranoKurwusBot.Core.Python;

public partial class PythonManager
{
    public static Command InstallPackage(string packageName)
    {
        return Cli
            .Wrap(PipPath)
            .WithArguments($"install --upgrade-strategy eager --upgrade {packageName}");
    }
}