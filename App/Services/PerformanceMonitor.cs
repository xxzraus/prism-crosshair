using System.Diagnostics;
using System.Globalization;
using System.Windows.Threading;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Services;

public sealed class PerformanceMonitor : IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly Process _process = Process.GetCurrentProcess();
    private TimeSpan _lastCpuTime;
    private DateTime _lastSample;
    private bool _started;

    public PerformanceMonitor()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (_, _) => Sample();
    }

    public void Start()
    {
        if (_started)
            return;
        _started = true;
        _process.Refresh();
        _lastCpuTime = _process.TotalProcessorTime;
        _lastSample = DateTime.UtcNow;
        _timer.Start();
        Sample();
    }

    private void Sample()
    {
        try
        {
            _process.Refresh();
            var now = DateTime.UtcNow;
            var cpuTime = _process.TotalProcessorTime;
            var elapsed = (now - _lastSample).TotalMilliseconds;
            var cpuMs = (cpuTime - _lastCpuTime).TotalMilliseconds;
            var percent = elapsed > 0 ? cpuMs / (elapsed * Environment.ProcessorCount) * 100 : 0;

            var state = AppState.Instance;
            state.MemoryText = $"{_process.PrivateMemorySize64 / 1024.0 / 1024.0:0} {Loc.T("MB")}";
            state.CpuText = string.Format(CultureInfo.InvariantCulture, "{0:0.0}%", percent);

            _lastCpuTime = cpuTime;
            _lastSample = now;
        }
        catch
        {
            // telemetry is non-critical
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _process.Dispose();
    }
}
