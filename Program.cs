using TYTDLPCS.Downloaders;
using TYTDLPCS.Python;

var path = PythonManager.PythonPath;

Console.WriteLine($"[{path}]");

var succeed = await DownloadManager.YtDlp.InstallOrUpdateAsync();

Console.WriteLine($"Sucess : {succeed}");
