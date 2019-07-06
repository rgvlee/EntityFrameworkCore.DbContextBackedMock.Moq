using Microsoft.EntityFrameworkCore;
using System;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbContext mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    public class DbContextMockBuilder<TDbContext> : MockBuilderBase<TDbContext>
        where TDbContext : DbContext {
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="addSetUpForAllDbSets">If set to true the DbContext DbSets will be set up automatically.</param>
        /// <remarks>Automatically creates a new in-memory database that will be used to back the DbContext mock.
        /// Requires the <see>
        ///     <cref>TDbContext</cref>
        /// </see>
        /// type to have a DbContextOptions constructor.</remarks>
        public DbContextMockBuilder(bool addSetUpForAllDbSets = true) {
            var options = new DbContextOptionsBuilder<TDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            DbContextToMock = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options);
            DbContextMock = DbContextToMock.CreateDbContextMock();
            if (addSetUpForAllDbSets) AddSetUpForAllDbSets();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        /// <param name="addSetUpForAllDbSets">If set to true the DbContext DbSets will be set up automatically.</param>
        public DbContextMockBuilder(TDbContext dbContextToMock, bool addSetUpForAllDbSets = true) : base(dbContextToMock, dbContextToMock.CreateDbContextMock()) {
            if (addSetUpForAllDbSets) AddSetUpForAllDbSets();
        }
    }
}