using System;
using System.Linq;
using System.Windows.Forms;
using EbayDesk.Core;

namespace EbayDesk.UI
{
    public class AccountsTab : UserControl
    {
        private readonly Storage _store;
        private DataGridView _grid;
        private Button _btnAdd, _btnEdit, _btnDelete, _btnSetDefault;

        public AccountsTab(Storage store)
        {
            _store = store;
            Dock = DockStyle.Fill;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            _btnAdd = new Button { Text = "Aggiungi" };
            _btnEdit = new Button { Text = "Modifica" };
            _btnDelete = new Button { Text = "Elimina" };
            _btnSetDefault = new Button { Text = "Imposta Default" };
            top.Controls.AddRange(new Control[] { _btnAdd, _btnEdit, _btnDelete, _btnSetDefault });

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = "Id" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nome", DataPropertyName = "Name" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mode", DataPropertyName = "Mode" });
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Default", DataPropertyName = "IsDefault" });

            Controls.Add(_grid);
            Controls.Add(top);

            _btnAdd.Click += (s, e) => EditAccount(null);
            _btnEdit.Click += (s, e) => { if (_grid.CurrentRow?.DataBoundItem is Account a) EditAccount(a); };
            _btnDelete.Click += (s, e) => {
                if (_grid.CurrentRow?.DataBoundItem is Account a) { _store.Accounts.Delete(a.Id); Reload(); }
            };
            _btnSetDefault.Click += (s, e) => {
                if (_grid.CurrentRow?.DataBoundItem is Account a)
                {
                    foreach (var x in _store.Accounts.FindAll())
                    {
                        x.IsDefault = x.Id == a.Id;
                        _store.Accounts.Update(x);
                    }
                    Reload();
                }
            };

            Reload();
        }

        private void Reload()
        {
            var list = _store.Accounts.FindAll()
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Name)
                .ToList();
            _grid.DataSource = list;
        }

        private void EditAccount(Account? acc)
        {
            var dlg = new Form
            {
                Text = acc == null ? "Nuovo account" : "Modifica account",
                Width = 500, Height = 380
            };

            var lbls = new[] { "Nome", "Mode", "Env", "AppId", "DevId", "CertId", "AuthToken" };
            var controls = new Control[] {
                new TextBox { Left = 150, Top = 20, Width = 300, Text = acc?.Name ?? "" },
                new ComboBox { Left = 150, Top = 60, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList },
                new ComboBox { Left = 150, Top = 100, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList },
                new TextBox { Left = 150, Top = 140, Width = 300, Text = acc?.AppId ?? "" },
                new TextBox { Left = 150, Top = 170, Width = 300, Text = acc?.DevId ?? "" },
                new TextBox { Left = 150, Top = 200, Width = 300, Text = acc?.CertId ?? "" },
                new TextBox { Left = 150, Top = 230, Width = 300, Text = acc?.AuthToken ?? "" }
            };

            var cboMode = (ComboBox)controls[1];
            cboMode.Items.AddRange(new[] { "MOCK", "SANDBOX", "PROD" });
            cboMode.SelectedItem = acc?.Mode ?? "MOCK";

            var cboEnv = (ComboBox)controls[2];
            cboEnv.Items.AddRange(new[] { "PRODUCTION", "SANDBOX" });
            cboEnv.SelectedItem = acc?.Env ?? "PRODUCTION";

            var chkDefault = new CheckBox
            {
                Left = 150, Top = 270, Text = "Imposta come predefinito", Checked = acc?.IsDefault ?? false
            };

            var btnSave = new Button { Left = 150, Top = 310, Width = 100, Text = "Salva" };
            btnSave.Click += (s, e) =>
            {
                var a = acc ?? new Account();
                a.Name = ((TextBox)controls[0]).Text.Trim();
                a.Mode = cboMode.SelectedItem.ToString()!;
                a.Env = cboEnv.SelectedItem.ToString()!;
                a.AppId = ((TextBox)controls[3]).Text.Trim();
                a.DevId = ((TextBox)controls[4]).Text.Trim();
                a.CertId = ((TextBox)controls[5]).Text.Trim();
                a.AuthToken = ((TextBox)controls[6]).Text.Trim();
                if (acc == null) _store.Accounts.Insert(a); else _store.Accounts.Update(a);
                if (chkDefault.Checked)
                {
                    foreach (var x in _store.Accounts.FindAll())
                    {
                        x.IsDefault = x.Id == a.Id;
                        _store.Accounts.Update(x);
                    }
                }
                dlg.DialogResult = DialogResult.OK;
                dlg.Close();
            };

            for (int i = 0; i < lbls.Length; i++)
            {
                dlg.Controls.Add(new Label { Left = 20, Top = 20 + i * 40, Text = lbls[i] });
                dlg.Controls.Add(controls[i]);
            }
            dlg.Controls.Add(chkDefault);
            dlg.Controls.Add(btnSave);

            if (dlg.ShowDialog() == DialogResult.OK) Reload();
        }
    }
}
