using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayDesk.Core;

namespace EbayDesk.Ebay
{
    public class EbayClient
    {
        private readonly AppConfig _cfg;
        public EbayClient(AppConfig cfg) => _cfg = cfg;

        private (string Mode, string Env, string Token) Resolve(Core.Account? acc)
        {
            if (acc != null && !string.IsNullOrWhiteSpace(acc.AuthToken))
                return (acc.Mode, acc.Env, acc.AuthToken);
            return (_cfg.Ebay.Mode, _cfg.Ebay.Env, _cfg.Ebay.AuthToken);
        }

        public async Task<Dictionary<string, object>> RevisePriceQtyAsync(
            string itemId,
            double? price,
            int? qty,
            Core.Account? account = null,
            CancellationToken ct = default)
        {
            var acc = Resolve(account);
            if (acc.Mode.ToUpperInvariant() == "MOCK")
                return new() { ["status"] = "ok", ["mode"] = "MOCK", ["itemId"] = itemId, ["price"] = price, ["qty"] = qty };

            // Costruisco il corpo XML con stringa verbatim per evitare problemi di escape
            var xml = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<ReviseItemRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">
  <RequesterCredentials>
    <eBayAuthToken>" + SecurityElement.Escape(acc.Token) + @"</eBayAuthToken>
  </RequesterCredentials>
  <Item>
    <ItemID>" + SecurityElement.Escape(itemId) + @"</ItemID>" +
    (price.HasValue ? @"
    <StartPrice>" + price.Value.ToString("0.00") + @"</StartPrice>" : "") +
    (qty.HasValue   ? @"
    <Quantity>"   + qty.Value               + @"</Quantity>"   : "") + @"
  </Item>
</ReviseItemRequest>";

            var endpoint = acc.Env.ToUpperInvariant() == "PRODUCTION"
                ? "https://api.ebay.com/ws/api.dll"
                : "https://api.sandbox.ebay.com/ws/api.dll";

            using var http = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.Add("X-EBAY-API-CALL-NAME", "ReviseItem");
            req.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", "1149");
            req.Headers.Add("X-EBAY-API-SITEID", "77");
            req.Content = new StringContent(xml, Encoding.UTF8, "text/xml");

            var resp = await http.SendAsync(req, ct);
            var responseXml = await resp.Content.ReadAsStringAsync(ct);
            return new()
            {
                ["status"]   = resp.IsSuccessStatusCode ? "ok" : "error",
                ["response"] = responseXml
            };
        }
    }
}
