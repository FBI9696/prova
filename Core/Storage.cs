using LiteDB;

namespace EbayDesk.Core {
  public class Storage : IDisposable {
    private readonly LiteDatabase _db;
    public ILiteCollection<Product> Products => _db.GetCollection<Product>("products");
    public ILiteCollection<RunLog> Logs => _db.GetCollection<RunLog>("logs");
    public ILiteCollection<Account> Accounts => _db.GetCollection<Account>("accounts");
    public ILiteCollection<ListingTemplate> Templates => _db.GetCollection<ListingTemplate>("templates");

    public Storage(string path) {
      _db = new LiteDatabase(path);
      Products.EnsureIndex(p => p.Id, true);
      Logs.EnsureIndex(l => l.Id, true);
      Accounts.EnsureIndex(a => a.Id, true);
      Templates.EnsureIndex(t => t.Id, true);
    }

    public void Dispose() => _db.Dispose();
  }
}
