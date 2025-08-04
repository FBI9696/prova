using System;

namespace EbayDesk.Core {
  public class Product {
    public int Id { get; set; }
    public string EbayItemId { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string SupplierUrl { get; set; } = "";
    public bool AutoReprice { get; set; } = true;
    public bool AutoStock { get; set; } = true;
    public double? LastSupplierPrice { get; set; }
    public bool? LastSupplierInStock { get; set; }
    public double? LastEbayPrice { get; set; }
    public int? LastEbayQty { get; set; }
    public int? AccountId { get; set; }
  }

  public class RunLog {
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Level { get; set; } = "INFO";
    public string Message { get; set; } = "";
    public int? ProductId { get; set; }
  }
}
