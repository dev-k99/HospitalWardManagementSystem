using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WardSystemProject.Data;

namespace WardSystemProject.Tests.Helpers
{
    /// <summary>
    /// Creates an in-memory EF Core DbContext for unit tests.
    /// Each test gets a uniquely-named database so tests don't share state.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static WardSystemDBContext Create(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<WardSystemDBContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new WardSystemDBContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
