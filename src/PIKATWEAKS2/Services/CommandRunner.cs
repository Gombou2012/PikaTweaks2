using System.Diagnostics;
using System.Text;
namespace PIKATWEAKS2.Services;
public sealed class CommandRunner
{
    public async Task<(int ExitCode,string Output)> RunAsync(string command, CancellationToken token=default)
    {
        var psi = new ProcessStartInfo("cmd.exe", "/d /s /c " + command)
        {
            UseShellExecute=false, CreateNoWindow=true, RedirectStandardOutput=true, RedirectStandardError=true,
            StandardOutputEncoding=Encoding.UTF8, StandardErrorEncoding=Encoding.UTF8
        };
        using var p = new Process { StartInfo=psi, EnableRaisingEvents=true };
        var output = new StringBuilder();
        p.OutputDataReceived += (_,e)=>{ if(e.Data!=null) output.AppendLine(e.Data); };
        p.ErrorDataReceived += (_,e)=>{ if(e.Data!=null) output.AppendLine(e.Data); };
        p.Start(); p.BeginOutputReadLine(); p.BeginErrorReadLine();
        await p.WaitForExitAsync(token);
        return (p.ExitCode, output.ToString());
    }
}
