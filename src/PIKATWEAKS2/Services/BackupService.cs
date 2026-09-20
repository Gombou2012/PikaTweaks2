using System.IO;
namespace PIKATWEAKS2.Services;
public sealed class BackupService
{
    public string Folder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PIKATWEAKS2", "Backups");
    public async Task<string> CreateAsync(CommandRunner runner)
    {
        Directory.CreateDirectory(Folder);
        var dir = Path.Combine(Folder, DateTime.Now.ToString("yyyyMMdd-HHmmss")); Directory.CreateDirectory(dir);
        var commands = new[] {
            $"reg export \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\" \"{Path.Combine(dir,"Personalize.reg")}\" /y",
            $"reg export \"HKCU\\Control Panel\\Desktop\" \"{Path.Combine(dir,"Desktop.reg")}\" /y",
            $"reg export \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\" \"{Path.Combine(dir,"ExplorerAdvanced.reg")}\" /y",
            $"reg export \"HKCU\\Software\\Microsoft\\GameBar\" \"{Path.Combine(dir,"GameBar.reg")}\" /y",
            $"reg export \"HKCU\\System\\GameConfigStore\" \"{Path.Combine(dir,"GameConfigStore.reg")}\" /y",
            $"reg export \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers\" \"{Path.Combine(dir,"GraphicsDrivers.reg")}\" /y"
        };
        foreach(var c in commands) await runner.RunAsync(c);
        await File.WriteAllTextAsync(Path.Combine(dir,"created.txt"), DateTime.Now.ToString("O"));
        return dir;
    }
}
