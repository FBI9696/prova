using HtmlAgilityPack;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EbayDesk.Suppliers {
  public static class PriceParser {
    public static double? Parse(string text) {
      if (string.IsNullOrWhiteSpace(text)) return null;
      text = text.Replace(((char)160).ToString(), " ").Replace(".", "").Trim();
      var m = System.Text.RegularExpressions.Regex.Match(text, @"(\d+),(\d+)");
      if (!m.Success) return null;
      var euros = int.Parse(m.Groups[1].Value);
      var cents = int.Parse(m.Groups[2].Value);
      return euros + cents / 100.0;
    }
  }
}
