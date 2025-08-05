using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EbayDesk.Core;

namespace EbayDesk.UI
{
    public class ListerTab : UserControl
    {
        private readonly Storage _store;
        private readonly AppConfig _cfg;
        private DataGridView _grid;

        public ListerTab(Storage store, AppConfig cfg)
        {
            _store = store;
            _cfg = cfg;
            Dock = DockStyle.Fill;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            var btnTemplate = new Button { Text = "Template" };
            var btnNew = new Button { Text = "Nuovo listing" };
            top.Controls.AddRange(new Control[] { btnTemplate, btnNew });

            btnTemplate.Click += (s, e) => MessageBox.Show("Da implementare gestione template");
            btnNew.Click += (s, e) => MessageBox.Show("Da implementare creazione listing");

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome", DataPropertyName = "Name" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Categoria", DataPropertyName = "DefaultCategoryId" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Condizione", DataPropertyName = "DefaultConditionId" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Prezzo cents", DataPropertyName = "DefaultImagesCsv" });

            Controls.Add(_grid);
            Controls.Add(top);

            Reload();
        }

        private void Reload()
        {
            var list = _store.Templates.FindAll().ToList();
            _grid.DataSource = list;
        }
    }
}
