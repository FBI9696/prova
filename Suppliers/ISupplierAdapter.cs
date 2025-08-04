using EbayDesk.Core;
using System.Threading;
using System.Threading.Tasks;

namespace EbayDesk.Suppliers {
  public interface ISupplierAdapter {
    Task<SupplierResult> FetchAsync(string url, CancellationToken ct = default);
  }

  public record SupplierResult(double? Price, bool InStock, string Raw);
}
