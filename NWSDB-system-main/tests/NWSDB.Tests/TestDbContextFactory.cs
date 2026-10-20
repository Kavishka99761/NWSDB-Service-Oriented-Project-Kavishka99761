using Microsoft.EntityFrameworkCore;
using NWSDB.Data;

namespace NWSDB.Tests;

public static class TestDbContextFactory
{
    public static NwsdbDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<NwsdbDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new NwsdbDbContext(options);
        DbInitializer.Initialize(context);
        return context;
    }
}
