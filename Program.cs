using System; using System.Windows.Forms;
namespace EbayDesk {
  internal static class Program {
    [STAThread]
    static void Main() {
      ApplicationConfiguration.Initialize();
      Application.Run(new UI.MainForm());
    }
  }
}
