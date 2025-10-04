using Microsoft.EntityFrameworkCore;

namespace VDT.Lock.Api.Data;

public class LockContext : DbContext {
    public DbSet<DataStore> DataStores => Set<DataStore>();

    public LockContext(DbContextOptions<LockContext> options) : base(options) { }
}
