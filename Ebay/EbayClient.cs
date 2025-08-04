using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayDesk.Core;

namespace EbayDesk.Ebay {
  public class EbayClient {
    private readonly AppConfig _cfg;
    public EbayClient(AppConfig cfg) => _cfg = cfg;

    private (string Mode, string Env, string Token) Resolve(Core.Account? acc) {
      if (acc != null && !string.IsNullOrWhiteSpace(acc.AuthToken))
        return (acc.Mode, acc.Env, acc.AuthToken);
      return (_cfg.Ebay.Mode, _cfg.Ebay.Env, _cfg.Ebay.AuthToken);
    }

    public async Task<Dictionary<string,object>> RevisePriceQtyAsync(string itemId, double? price, int? qty, Core.Account? account = null, CancellationToken ct = default) {
      var acc = Resolve(account);
      if (acc.Mode.ToUpperInvariant() == "MOCK")
        return new() { ["status"] = "ok", ["mode"] = "MOCK", ["itemId"] = itemId, ["price"] = price, ["qty"] = qty };

      var endpoint = acc.Env.ToUpperInvariant() == "PRODUCTION" 
        ? "https://api.ebay.com/ws/api.dll" 
        : "https://api.sandbox.ebay.com/ws/api.dll";

      var headers = new Dictionary<string,string> {
        ["X-EBAY-API-CALL-NAME"] = "ReviseItem",
        ["X-EBAY-API-COMPATIBILITY-LEVEL"] = "1149",
        ["X-EBAY-API-SITEID"] = "77"
      };

      var sb = new StringBuilder();
      sb.Append($@"<?xml version=\"1.0\" encoding=\"utf-8\"?>
<ReviseItemRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">
  <RequesterCredentials>
    <eBayAuthToken>{System.Security.SecurityElement.Escape(acc.Token)}</eBayAuthToken>
  </RequesterCredentials>
  <Item><ItemID>{System.Security.SecurityElement.Escape(itemId)}</ItemID>");
      if (price.HasValue) sb.Append($@"<StartPrice>{price.Value:0.00}</StartPrice>");
      if (qty.HasValue) sb.Append($@"<Quantity>{qty.Value}</Quantity>");
      sb.Append("</Item></ReviseItemRequest>");

      using var http = new HttpClient();
      var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
      foreach (var h in headers) req.Headers.TryAddWithoutValidation(h.Key, h.Value);
      req.Content = new StringContent(sb.ToString(), Encoding.UTF8, "text/xml");
      var resp = await http.SendAsync(req, ct);
      var xml = await resp.Content.ReadAsStringAsync(ct);
      return new() { ["status"] = resp.IsSuccessStatusCode ? "ok" : "error", ["response"] = xml };
    }
  }
}
