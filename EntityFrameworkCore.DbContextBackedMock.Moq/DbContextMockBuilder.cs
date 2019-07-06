using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbContext mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    public class DbContextMockBuilder<TDbContext> : MockBuilderBase<TDbContext> where TDbContext : DbContext {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>Automatically creates a new in-memory database that will be used to back the DbContext mock.
        /// Requires the <see>
        ///     <cref>TDbContext</cref>
        /// </see>
        /// type to have a DbContextOptions constructor.</remarks>
        public DbContextMockBuilder() : base () { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        public DbContextMockBuilder(TDbContext dbContextToMock) : base(dbContextToMock) { }
    }
}