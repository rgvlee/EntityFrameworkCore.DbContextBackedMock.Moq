using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    public class TestContext : DbContext {

        public DbSet<TestEntity1> DbSetEntities { get; set; }
        public DbQuery<TestEntity2> DbQueryEntities { get; set; }

        public TestContext() {

        }

        public TestContext(DbContextOptions<TestContext> options) : base(options) {

        }
    }
}