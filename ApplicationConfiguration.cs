using System; using System.Windows.Forms;
namespace EbayDesk {
  internal static class ApplicationConfiguration {
    public static void Initialize() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
    }
  }
}
