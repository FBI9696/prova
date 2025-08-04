using System;
using System.Timers;
using EbayDesk.Core;

namespace EbayDesk.Tasks {
  public class SchedulerService : IDisposable {
    private readonly Timer _timer;
    private readonly Storage _store;
    private readonly AppConfig _cfg;
    private readonly Runner _runner;
    private bool _running;

    public SchedulerService(Storage store, AppConfig cfg) {
      _store = store;
      _cfg = cfg;
      _runner = new Runner(_store, _cfg);
      _timer = new Timer(cfg.Scheduler.Minutes * 60000);
      _timer.Elapsed += async (s, e) => await TickAsync();
    }

    public bool IsActive => _timer.Enabled;
    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private async Task TickAsync() {
      if (_running) return;
      _running = true;
      try { await _runner.RunAllAsync(); }
      finally { _running = false; }
    }

    public void Dispose() => _timer.Dispose();
  }
}
