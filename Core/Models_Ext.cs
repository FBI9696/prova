namespace EbayDesk.Core {
  public class Account {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Mode { get; set; } = "MOCK";
    public string Env { get; set; } = "PRODUCTION";
    public string AppId { get; set; } = "";
    public string DevId { get; set; } = "";
    public string CertId { get; set; } = "";
    public string AuthToken { get; set; } = "";
    public bool IsDefault { get; set; } = false;
  }

  public class ListingTemplate {
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string TitleTemplate { get; set; } = "{title}";
    public string DescriptionTemplate { get; set; } = "{description}";
    public string Currency { get; set; } = "EUR";
    public int DefaultCategoryId { get; set; } = 0;
    public int DefaultConditionId { get; set; } = 1000;
    public string DefaultImagesCsv { get; set; } = "";
  }
}
