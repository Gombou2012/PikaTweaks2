using Microsoft.Win32;
namespace PikaTweaks.Services;
public sealed class BackupService
{
    public string Root { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PikaTweaks", "Backups");
    public BackupService() => Directory.CreateDirectory(Root);
    public string CreateSessionBackup(IEnumerable<string> registryRoots)
    {
        var folder = Path.Combine(Root, DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(folder);
        foreach (var root in registryRoots.Distinct())
        {
            try
            {
                var safe = string.Concat(root.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
                var file = Path.Combine(folder, safe + ".reg");
                var (hive, sub) = root.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase) ? ("HKCU", root[5..]) : ("HKLM", root[5..]);
                using var key = (hive == "HKCU" ? Registry.CurrentUser : Registry.LocalMachine).OpenSubKey(sub);
                if (key == null) continue;
                File.WriteAllText(file, "; PikaTweaks backup\r\n; Registry path: " + root + "\r\n");
            }
            catch { }
        }
        return folder;
    }
}
