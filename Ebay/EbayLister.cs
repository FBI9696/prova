using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayDesk.Core;

namespace EbayDesk.Ebay
{
    public class EbayLister
    {
        private readonly AppConfig _cfg;
        public EbayLister(AppConfig cfg) => _cfg = cfg;

        private (string Mode, string Env, string Token) Resolve(Core.Account? acc)
        {
            if (acc != null && !string.IsNullOrWhiteSpace(acc.AuthToken))
                return (acc.Mode, acc.Env, acc.AuthToken);
            return (_cfg.Ebay.Mode, _cfg.Ebay.Env, _cfg.Ebay.AuthToken);
        }

        public async Task<Dictionary<string, object>> AddFixedPriceItemAsync(
            Core.Account? account,
            string title, string description, string currency,
            int categoryId, int conditionId, double price, int quantity,
            IEnumerable<string> imageUrls,
            CancellationToken ct = default)
        {
            var acc = Resolve(account);
            if (acc.Mode.ToUpperInvariant() == "MOCK")
                return new() { ["status"] = "ok", ["mode"] = "MOCK", ["title"] = title, ["price"] = price, ["qty"] = quantity };

            // Costruisco XML dinamicamente evitando delegate ambigui
            var sb = new StringBuilder();
            sb.AppendLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            sb.AppendLine(@"<AddFixedPriceItemRequest xmlns=""urn:ebay:apis:eBLBaseComponents"">");
            sb.AppendLine(@"  <RequesterCredentials>");
            sb.AppendLine("    <eBayAuthToken>" + SecurityElement.Escape(acc.Token) + "</eBayAuthToken>");
            sb.AppendLine(@"  </RequesterCredentials>");
            sb.AppendLine(@"  <Item>");
            sb.AppendLine("    <Title>" + SecurityElement.Escape(title) + "</Title>");
            sb.AppendLine("    <Description><![CDATA[" + description + "]]></Description>");
            sb.AppendLine("    <PrimaryCategory><CategoryID>" + categoryId + "</CategoryID></PrimaryCategory>");
            sb.AppendLine("    <ConditionID>" + conditionId + "</ConditionID>");
            sb.AppendLine("    <StartPrice>" + price.ToString("0.00") + "</StartPrice>");
            sb.AppendLine("    <Currency>" + currency + "</Currency>");
            sb.AppendLine("    <Country>IT</Country>");
            sb.AppendLine("    <DispatchTimeMax>3</DispatchTimeMax>");
            sb.AppendLine("    <ListingDuration>GTC</ListingDuration>");
            sb.AppendLine("    <ListingType>FixedPriceItem</ListingType>");
            sb.AppendLine("    <Quantity>" + quantity + "</Quantity>");
            sb.AppendLine("    <ReturnPolicy><ReturnsAcceptedOption>ReturnsAccepted</ReturnsAcceptedOption></ReturnPolicy>");
            sb.AppendLine("    <PictureDetails>");
            foreach (var url in imageUrls)
                sb.AppendLine("      <PictureURL>" + SecurityElement.Escape(url) + "</PictureURL>");
            sb.AppendLine("    </PictureDetails>");
            sb.AppendLine(@"  </Item>");
            sb.AppendLine(@"</AddFixedPriceItemRequest>");

            var xml = sb.ToString();
            var endpoint = acc.Env.ToUpperInvariant() == "PRODUCTION"
                ? "https://api.ebay.com/ws/api.dll"
                : "https://api.sandbox.ebay.com/ws/api.dll";

            using var http = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.Add("X-EBAY-API-CALL-NAME", "AddFixedPriceItem");
            req.Headers.Add("X-EBAY-API-COMPATIBILITY-LEVEL", "1149");
            req.Headers.Add("X-EBAY-API-SITEID", "77");
            req.Content = new StringContent(xml, Encoding.UTF8);
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

            var resp = await http.SendAsync(req, ct);
            var responseXml = await resp.Content.ReadAsStringAsync(ct);
            return new()
            {
                ["status"] = resp.IsSuccessStatusCode ? "ok" : "error",
                ["response"] = responseXml
            };
        }
    }
}
