using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbSet mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
    public class DbSetMockBuilder<TDbContext, TEntity> : MockBuilderBase<TDbContext>
        where TDbContext : DbContext
        where TEntity : class {

        /// <summary>
        /// The DbSet to mock.
        /// </summary>
        protected DbSet<TEntity> DbSetToMock;

        /// <summary>
        /// The DbSet mock.
        /// </summary>
        protected Mock<DbSet<TEntity>> DbSetMock;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        /// <param name="dbContextMock">The DbContext mock.</param>
        internal DbSetMockBuilder(TDbContext dbContextToMock, Mock<TDbContext> dbContextMock) {
            DbContextToMock = dbContextToMock;
            DbContextMock = dbContextMock;

            DbSetToMock = DbContextToMock.Set<TEntity>();
            DbSetMock = DbSetToMock.CreateDbSetMock();
            
            DbContextMock.Setup(m => m.Set<TEntity>()).Returns(() => DbSetMock.Object);
            
            DbContextMock.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => DbContextToMock.Add(entity));
            DbContextMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<object>>()))
                .Callback((IEnumerable<object> entities) => DbContextToMock.AddRange(entities));
            DbContextMock.Setup(m => m.AddRange(It.IsAny<object[]>()))
                .Callback((object[] entities) => DbContextToMock.AddRange(entities));

            DbContextMock.Setup(m => m.Find<TEntity>(It.IsAny<object[]>()))
                .Returns((object[] keyValues) => DbSetMock.Object.Find(keyValues));
            DbContextMock.Setup(m => m.Find(typeof(TEntity), It.IsAny<object[]>()))
                .Returns((Type type, object[] keyValues) => DbSetMock.Object.Find(type, keyValues));

            DbContextMock.Setup(m => m.Remove(It.IsAny<object>())).Returns((object entity) => DbContextToMock.Remove(entity));
            DbContextMock.Setup(m => m.Remove(It.IsAny<TEntity>()))
                .Returns((TEntity entity) => DbContextToMock.Remove(entity));
            DbContextMock.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<object>>()))
                .Callback((IEnumerable<object> entities) => DbContextToMock.RemoveRange(entities));
            DbContextMock.Setup(m => m.RemoveRange(It.IsAny<object[]>()))
                .Callback((object[] entities) => DbContextToMock.RemoveRange(entities));

            DbContextMock.Setup(m => m.Update(It.IsAny<object>())).Returns((object entity) => DbContextToMock.Update(entity));
            DbContextMock.Setup(m => m.Update(It.IsAny<TEntity>()))
                .Returns((TEntity entity) => DbContextToMock.Update(entity));
            DbContextMock.Setup(m => m.UpdateRange(It.IsAny<IEnumerable<object>>()))
                .Callback((IEnumerable<object> entities) => DbContextToMock.UpdateRange(entities));
            DbContextMock.Setup(m => m.UpdateRange(It.IsAny<object[]>()))
                .Callback((object[] entities) => DbContextToMock.UpdateRange(entities));
        }

        /// <summary>
        /// Adds the specified query provider mock to the DbSet mock set up for the specified entity.
        /// </summary>
        /// <typeparam>The DbSet entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock to add.</param>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> WithQueryProviderMock(Mock<IQueryProvider> queryProviderMock) {
            DbSetMock.AddSetUpForProvider(queryProviderMock);
            return this;
        }

        /// <summary>
        /// Adds the specified expected FromSql result to the DbSet mock set up for the specified entity.
        /// </summary>
        /// <typeparam>The DbSet entity type.</typeparam>
        /// <param name="expectedFromSqlResult">The expected FromSql result.</param>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> WithFromSqlResult(IEnumerable<TEntity> expectedFromSqlResult) {
            DbSetMock.AddSetUpForMockFromSql(expectedFromSqlResult);
            return this;
        }

        /// <summary>
        /// Gets the set up DbSet mock for the specified entity.
        /// </summary>
        /// <typeparam>The DbSet entity type.</typeparam>
        /// <returns>The DbSet mock for the specified entity.</returns>
        internal Mock<DbSet<TEntity>> CreateDbSetMock() {
            return DbSetMock;
        }
    }
}