using System.Threading;
using System.Threading.Tasks;

namespace EbayDesk.Suppliers {
  public class BekoVipAdapter : ISupplierAdapter {
    private readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = true });

    public async Task<SupplierResult> FetchAsync(string url, CancellationToken ct = default) {
      var html = await _http.GetStringAsync(url, ct);
      var doc = new HtmlAgilityPack.HtmlDocument();
      doc.LoadHtml(html);
      var priceNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'price') or contains(@class,'product-price')]");
      var stockNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'stock') or contains(@class,'availability')]");
      var price = PriceParser.Parse(priceNode?.InnerText?.Trim() ?? "");
      var inStock = (stockNode?.InnerText ?? "").ToLowerInvariant().Contains("disponibile");
      return new(price, inStock, html);
    }
  }
}
