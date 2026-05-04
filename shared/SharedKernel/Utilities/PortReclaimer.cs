using System.Diagnostics;

namespace SharedKernel.Utilities;

public static class PortReclaimer
{
    /// <summary>
    /// Checks if a port is in use and terminates the owning process if it's not the current one.
    /// This is particularly useful in development environments to handle "ghost" processes.
    /// </summary>
    public static void Reclaim(int portNumber)
    {
        try
        {
            // Use PowerShell to find the PID owning the port
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-NoProfile -Command \"Get-NetTCPConnection -LocalPort {portNumber} -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output) && int.TryParse(output, out int pid))
            {
                var currentPid = Process.GetCurrentProcess().Id;
                if (pid != currentPid)
                {
                    Console.WriteLine($"[PortReclaimer] ⚠️ Port {portNumber} is busy (PID: {pid}). Reclaiming...");
                    try
                    {
                        var targetProcess = Process.GetProcessById(pid);
                        targetProcess.Kill();
                        targetProcess.WaitForExit(2000); // Wait up to 2 seconds for exit
                        Thread.Sleep(500); // Small buffer for OS socket release
                        Console.WriteLine($"[PortReclaimer] ✅ Port {portNumber} reclaimed.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[PortReclaimer] ❌ Failed to kill process {pid}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Fail silently as this is a convenience utility for dev environments
            Console.WriteLine($"[PortReclaimer] ℹ️ Port check skipped: {ex.Message}");
        }
    }
}
