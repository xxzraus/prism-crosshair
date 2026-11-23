using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PrismCrosshair.Services;

public static class MemoryTrimmer
{
    [DllImport("psapi.dll", SetLastError = true)]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    public static void Trim()
    {
        try
        {
            EmptyWorkingSet(GetCurrentProcess());
        }
        catch
        {
            // Trimming is optional; ignore failures on restricted systems.
        }
    }

    public static string WorkingSetText()
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            return $"{process.WorkingSet64 / 1024.0 / 1024.0:0} MB";
        }
        catch
        {
            return "—";
        }
    }
}
