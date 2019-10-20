using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests
{
    public class TestContext : DbContext
    {
        public TestContext() { }

        public TestContext(DbContextOptions<TestContext> options) : base(options) { }

        public virtual DbSet<TestEntity1> TestEntities { get; set; }
        public virtual DbQuery<TestEntity2> TestView { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Query<TestEntity2>().ToView("SomeView");
        }
    }
}