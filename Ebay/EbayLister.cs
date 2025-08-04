using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayDesk.Core;

namespace EbayDesk.Ebay {
  public class EbayLister {
    private readonly AppConfig _cfg;
    public EbayLister(AppConfig cfg) => _cfg = cfg;

    private (string Mode, string Env, string Token) Resolve(Core.Account? acc) {
      if (acc != null && !string.IsNullOrWhiteSpace(acc.AuthToken))
        return (acc.Mode, acc.Env, acc.AuthToken);
      return (_cfg.Ebay.Mode, _cfg.Ebay.Env, _cfg.Ebay.AuthToken);
    }

    public async Task<Dictionary<string,object>> AddFixedPriceItemAsync(
      Core.Account? account,
      string title, string description, string currency,
      int categoryId, int conditionId, double price, int quantity,
      IEnumerable<string> imageUrls,
      CancellationToken ct = default) 
    {
      var acc = Resolve(account);
      if (acc.Mode.ToUpperInvariant() == "MOCK") 
        return new() { ["status"] = "ok", ["mode"] = "MOCK", ["title"] = title, ["price"] = price, ["qty"] = quantity };

      var endpoint = acc.Env.ToUpperInvariant() == "PRODUCTION"
        ? "https://api.ebay.com/ws/api.dll"
        : "https://api.sandbox.ebay.com/ws/api.dll";

      var headers = new Dictionary<string,string> {
        ["X-EBAY-API-CALL-NAME"] = "AddFixedPriceItem",
        ["X-EBAY-API-COMPATIBILITY-LEVEL"] = "1149",
        ["X-EBAY-API-SITEID"] = "77"
      };

      var sb = new StringBuilder();
      sb.Append($@"<?xml version=\"1.0\" encoding=\"utf-8\"?>
<AddFixedPriceItemRequest xmlns=\"urn:ebay:apis:eBLBaseComponents\">
  <RequesterCredentials>
    <eBayAuthToken>{System.Security.SecurityElement.Escape(acc.Token)}</eBayAuthToken>
  </RequesterCredentials>
  <Item>
    <Title>{System.Security.SecurityElement.Escape(title)}</Title>
    <Description><![CDATA[{description}]]></Description>
    <PrimaryCategory><CategoryID>{categoryId}</CategoryID></PrimaryCategory>
    <ConditionID>{conditionId}</ConditionID>
    <StartPrice>{price:0.00}</StartPrice>
    <Currency>{currency}</Currency>
    <Country>IT</Country>
    <DispatchTimeMax>3</DispatchTimeMax>
    <ListingDuration>GTC</ListingDuration>
    <ListingType>FixedPriceItem</ListingType>
    <Quantity>{quantity}</Quantity>
    <ReturnPolicy><ReturnsAcceptedOption>ReturnsAccepted</ReturnsAcceptedOption></ReturnPolicy>
    <PictureDetails>");
      foreach(var url in imageUrls) sb.Append($"<PictureURL>{System.Security.SecurityElement.Escape(url)}</PictureURL>");
      sb.Append("</PictureDetails></Item></AddFixedPriceItemRequest>");

      using var http = new HttpClient();
      var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
      foreach(var h in headers) req.Headers.TryAddWithoutValidation(h.Key, h.Value);
      req.Content = new StringContent(sb.ToString(), Encoding.UTF8, "text/xml");
      var resp = await http.SendAsync(req, ct);
      var xml = await resp.Content.ReadAsStringAsync(ct);
      return new() { ["status"] = resp.IsSuccessStatusCode ? "ok" : "error", ["response"] = xml };
    }
  }
}
