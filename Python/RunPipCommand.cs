using System.Diagnostics;

namespace TYTDLPCS.Python;

public static partial class PythonManager
{
    public static Process RunPipCommand(string command) {
        var pipCommand = CreateCliProcess(PipPath, command);
        pipCommand.Start();
        pipCommand.WaitForExit();
        return pipCommand;
    }

    // public static Process StartPipCommandAsync(string command) {
    //     var pipCommand = CreateCliProcess(PipPath, command);
    //     pipCommand.Start();
    //     return pipCommand;
    // }
}