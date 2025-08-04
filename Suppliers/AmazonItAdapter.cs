using System.Threading;
using System.Threading.Tasks;

namespace EbayDesk.Suppliers {
  public class AmazonItAdapter : ISupplierAdapter {
    private readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = true });

    public async Task<SupplierResult> FetchAsync(string url, CancellationToken ct = default) {
      var html = await _http.GetStringAsync(url, ct);
      var doc = new HtmlAgilityPack.HtmlDocument();
      doc.LoadHtml(html);
      var priceNode = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'a-offscreen')]");
      var stockNode = doc.DocumentNode.SelectSingleNode("//div[@id='availability']");
      var price = PriceParser.Parse(priceNode?.InnerText?.Trim() ?? "");
      var inStock = (stockNode?.InnerText ?? "").ToLowerInvariant().Contains("disponibile");
      return new(price, inStock, html);
    }
  }
}
