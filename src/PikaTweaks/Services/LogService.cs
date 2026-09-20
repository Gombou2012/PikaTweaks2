namespace PikaTweaks.Services;
public sealed class LogService
{
    public string FilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PikaTweaks", "pikatweaks.log");
    public LogService() => Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
    public void Write(string message) => File.AppendAllText(FilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
}
