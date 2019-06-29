using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ContextBackedMock.Moq.Tests {
    public class TestContext : DbContext {
        public DbSet<TestEntity1> TestEntities { get; set; }

        public TestContext() {

        }

        public TestContext(DbContextOptions<TestContext> options) : base(options) {

        }
    }
}