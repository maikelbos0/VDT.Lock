using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VDT.Lock.Api.Data;
using Xunit;


namespace VDT.Lock.Api.Tests.Data;

public class LockContextProvider {
    public static LockContext Provide() {
        var connection = new SqliteConnection("Filename=:memory:");
        var options = new DbContextOptionsBuilder<LockContext>().UseSqlite(connection).Options;
        var context = new LockContext(options);

        connection.Open();
        Assert.True(context.Database.EnsureCreated());

        return context;
    }
}
