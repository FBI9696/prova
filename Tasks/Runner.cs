using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EbayDesk.Core;
using EbayDesk.Suppliers;
using EbayDesk.Ebay;

namespace EbayDesk.Tasks {
  public class Runner {
    private readonly Storage _store;
    private readonly AppConfig _cfg;
    private readonly Dictionary<string, ISupplierAdapter> _adapters;

    public Runner(Storage store, AppConfig cfg) {
      _store = store;
      _cfg = cfg;
      _adapters = new() {
        ["amazon_it"] = new AmazonItAdapter(),
        ["beko_vip"] = new BekoVipAdapter()
      };
    }

    public async Task<Dictionary<string,object>> RunOneAsync(int productId, CancellationToken ct = default) {
      var p = _store.Products.FindById(productId) ?? throw new ArgumentException("Product not found");
      if (!_adapters.TryGetValue(p.Supplier, out var adapter)) throw new ArgumentException($"Unknown supplier: {p.Supplier}");

      var res = await adapter.FetchAsync(p.SupplierUrl, ct);
      _store.Logs.Insert(new RunLog { ProductId = p.Id, Message = $"Supplier fetched: price={res.Price}, inStock={res.InStock}" });

      p.LastSupplierPrice = res.Price;
      p.LastSupplierInStock = res.InStock;
      int targetQty = res.InStock ? 1 : 0;
      double? targetPrice = null;

      if (res.Price.HasValue && p.AutoReprice) {
        var cfgP = _cfg.Pricing;
        var profit = p.AdditionalProfit ?? cfgP.ProfitPercent;
        var add = p.AdditionalProfit ?? cfgP.AdditionalProfit;
        var pct = p.PercentFees ?? cfgP.PercentFees;
        var fix = p.FixedFees ?? cfgP.FixedFees;
        var cents = p.PriceCentsValue ?? cfgP.PriceCentsValue;
        targetPrice = AutoDSPricer.Calculate(res.Price.Value, profit, add, pct, fix, cents);
        p.LastEbayPrice = targetPrice;
      }
      if (p.AutoStock) p.LastEbayQty = targetQty;
      _store.Products.Update(p);

      var defaultAcc = _store.Accounts.FindOne(a => a.IsDefault);
      var acc = p.AccountId.HasValue ? _store.Accounts.FindById(p.AccountId.Value) ?? defaultAcc : defaultAcc;
      var ebay = new EbayClient(_cfg);
      var result = await ebay.RevisePriceQtyAsync(p.EbayItemId, targetPrice, p.AutoStock ? targetQty : null, acc, ct);
      _store.Logs.Insert(new RunLog { ProductId = p.Id, Message = $"eBay update: {System.Text.Json.JsonSerializer.Serialize(result)}" });
      return new() { ["supplier_price"]=res.Price, ["in_stock"]=res.InStock, ["target_price"]=targetPrice, ["target_qty"]=targetQty, ["ebay_result"]=result };
    }

    public async Task RunAllAsync(CancellationToken ct = default) {
      foreach(var p in _store.Products.FindAll().ToList()) {
        try { await RunOneAsync(p.Id, ct); }
        catch(Exception ex) { _store.Logs.Insert(new RunLog { ProductId = p.Id, Level = "ERROR", Message = ex.ToString() }); }
      }
    }
  }
}
