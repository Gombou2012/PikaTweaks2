using System.Diagnostics;
using System.Text;
namespace PikaTweaks.Services;
public sealed record CommandResult(int ExitCode, string Output);
public sealed class CommandRunner
{
    public async Task<CommandResult> RunScriptAsync(string script, CancellationToken token = default)
    {
        var dir = Path.Combine(Path.GetTempPath(), "PikaTweaks");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"tweak-{Guid.NewGuid():N}.cmd");
        var content = "@echo off\r\nsetlocal\r\n" + script + "\r\n";
        await File.WriteAllTextAsync(path, content, new UTF8Encoding(false), token);
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                Arguments = $"/d /s /c \"\"{path}\"\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = Environment.SystemDirectory
            };
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Could not start cmd.exe.");
            var stdout = p.StandardOutput.ReadToEndAsync(token);
            var stderr = p.StandardError.ReadToEndAsync(token);
            await p.WaitForExitAsync(token);
            var output = (await stdout) + (await stderr);
            return new CommandResult(p.ExitCode, output.Trim());
        }
        finally { try { File.Delete(path); } catch { } }
    }
}
