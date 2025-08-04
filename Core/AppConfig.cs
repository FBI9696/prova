using System.IO;
using System.Text.Json;

namespace EbayDesk.Core {
  public class PricingConfig { public double ProfitPercent { get; set; } = 10.0; public double AdditionalProfit { get; set; } = 0.0; public double PercentFees { get; set; } = 14.0; public double FixedFees { get; set; } = 0.0; public double PriceCentsValue { get; set; } = 0.99; public bool IncludeShippingPrice { get; set; } = false; }
  public class EbayConfig { public string Mode { get; set; } = "MOCK"; public string Env { get; set; } = "PRODUCTION"; public string AppId { get; set; } = ""; public string DevId { get; set; } = ""; public string CertId { get; set; } = ""; public string AuthToken { get; set; } = ""; }
  public class SchedulerConfig { public int Minutes { get; set; } = 15; }
  public class DatabaseConfig { public string Path { get; set; } = "data.db"; }

  public class AppConfig {
    public PricingConfig Pricing { get; set; } = new();
    public EbayConfig Ebay { get; set; } = new();
    public SchedulerConfig Scheduler { get; set; } = new();
    public DatabaseConfig Database { get; set; } = new();

    public static AppConfig Load(string path) {
      var json = File.ReadAllText(path);
      return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AppConfig();
    }
  }
}
