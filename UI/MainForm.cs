using System;
using System.Linq;
using System.Windows.Forms;
using EbayDesk.Core;
using EbayDesk.Tasks;

namespace EbayDesk.UI {
  public class MainForm : Form {
    private readonly AppConfig _cfg;
    private readonly Storage _store;
    private readonly SchedulerService _scheduler;
    private DataGridView _grid = default!;
    private TextBox _logBox = default!;

    public MainForm() {
      Text = "EbayDesk - eBay Automation (Windows)";
      Width = 1100; Height = 700;
      _cfg = AppConfig.Load("appsettings.json");
      _store = new Storage(_cfg.Database.Path);
      _scheduler = new SchedulerService(_store, _cfg);

      var tabs = new TabControl { Dock = DockStyle.Fill };
      tabs.TabPages.Add(BuildDashboardTab());
      tabs.TabPages.Add(BuildProductsTab());

      var accPage = new TabPage("Accounts");
      accPage.Controls.Add(new AccountsTab(_store) { Dock = DockStyle.Fill });
      tabs.TabPages.Add(accPage);

      var listerPage = new TabPage("Lister");
      listerPage.Controls.Add(new ListerTab(_store, _cfg) { Dock = DockStyle.Fill });
      tabs.TabPages.Add(listerPage);

      tabs.TabPages.Add(BuildLogsTab());
      Controls.Add(tabs);
    }

    private TabPage BuildDashboardTab() {
      var tp = new TabPage("Dashboard");
      var pnl = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
      var btnStart = new Button { Text = "Avvia scheduler" };
      var btnStop = new Button { Text = "Ferma scheduler" };
      var btnRunAll = new Button { Text = "Esegui tutti ora" };

      btnStart.Click += (s, e) => { _scheduler.Start(); MessageBox.Show("Scheduler avviato"); };
      btnStop.Click += (s, e) => { _scheduler.Stop(); MessageBox.Show("Scheduler fermato"); };
      btnRunAll.Click += async (s, e) => { var r = new Runner(_store, _cfg); await r.RunAllAsync(); MessageBox.Show("Esecuzione completata."); };

      pnl.Controls.AddRange(new Control[]{ btnStart, btnStop, btnRunAll });
      tp.Controls.Add(pnl);
      return tp;
    }

    private TabPage BuildProductsTab() {
      var tp = new TabPage("Prodotti");
      var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
      var btnAdd = new Button { Text = "Aggiungi" };
      var btnImport = new Button { Text = "Importa CSV" };
      var btnRunSel = new Button { Text = "Esegui selezionato" };
      var btnSetAcc = new Button { Text = "Set Account" };

      btnAdd.Click += (s, e) => AddProductDialog();
      btnImport.Click += (s, e) => ImportCsvDialog();
      btnRunSel.Click += async (s, e) => await RunSelectedAsync();
      btnSetAcc.Click += (s, e) => SetAccountForSelected();

      top.Controls.AddRange(new Control[]{ btnAdd, btnImport, btnRunSel, btnSetAcc });

      _grid = new DataGridView {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
      };
      _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id" });
      _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "eBay ItemID", DataPropertyName = "EbayItemId" });
      _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Fornitore", DataPropertyName = "Supplier" });
      _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "URL", DataPropertyName = "SupplierUrl", Width = 400 });
      _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "AutoPrice", DataPropertyName = "AutoReprice" });
      _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "AutoStock", DataPropertyName = "AutoStock" });
      _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "AccountId", DataPropertyName = "AccountId" });

      tp.Controls.Add(_grid);
      tp.Controls.Add(top);
      ReloadGrid();
      return tp;
    }

    private TabPage BuildLogsTab() {
      var tp = new TabPage("Log");
      var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
      var btnRefresh = new Button { Text = "Aggiorna" };
      btnRefresh.Click += (s, e) => LoadLogs();
      _logBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both };
      top.Controls.Add(btnRefresh);
      tp.Controls.Add(_logBox);
      tp.Controls.Add(top);
      LoadLogs();
      return tp;
    }

    private void ReloadGrid() {
      var lst = _store.Products.FindAll().OrderByDescending(p => p.Id).ToList();
      _grid.DataSource = lst;
    }

    private void LoadLogs() {
      var logs = _store.Logs.FindAll().OrderByDescending(l => l.Id).Take(300).ToList();
      _logBox.Lines = logs.Select(l => $"[{l.Id}] {l.CreatedAt:u} {l.Level}: {l.Message}").ToArray();
    }

    private void AddProductDialog() { /*...*/ }
    private void ImportCsvDialog() { /*...*/ }
    private async Task RunSelectedAsync() { /*...*/ }
    private void SetAccountForSelected() { /*...*/ }
    protected override void OnFormClosed(FormClosedEventArgs e) { _scheduler.Dispose(); _store.Dispose(); base.OnFormClosed(e); }
  }
}
