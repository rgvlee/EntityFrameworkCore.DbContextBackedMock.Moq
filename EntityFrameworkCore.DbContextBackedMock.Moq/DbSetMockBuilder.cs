using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbSet mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
    public class DbSetMockBuilder<TDbContext, TEntity> : DbContextMockBuilder<TDbContext>
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

            //DbSetToMock = DbContextToMock.Set<TEntity>();
            DbSetToMock =(DbSet<TEntity>)((IDbSetCache) dbContextToMock).GetOrAddSet(((IDbContextDependencies) dbContextToMock).SetSource, typeof(TEntity));
            DbSetMock = DbSetToMock.CreateDbSetMock();
            
            DbContextMock.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => DbContextToMock.Add(entity));
            DbContextMock.Setup(m => m.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>())).Returns((TEntity entity, CancellationToken cancellationToken) => DbContextToMock.AddAsync(entity, cancellationToken));
            DbContextMock.Setup(m => m.Attach(It.IsAny<TEntity>())).Returns((TEntity entity) => DbContextToMock.Attach(entity));
            DbContextMock.Setup(m => m.AttachRange(It.IsAny<object[]>())).Callback((object[] entities) => DbContextToMock.AttachRange(entities));
            DbContextMock.Setup(m => m.AttachRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => DbContextToMock.AttachRange(entities));
            DbContextMock.Setup(m => m.Entry(It.IsAny<TEntity>())).Returns((TEntity entity) => DbContextToMock.Entry(entity));
            DbContextMock.Setup(m => m.Find<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => DbContextToMock.Find<TEntity>(keyValues));
            DbContextMock.Setup(m => m.Find(typeof(TEntity), It.IsAny<object[]>())).Returns((Type type, object[] keyValues) => DbContextToMock.Find(type, keyValues));
            DbContextMock.Setup(m => m.FindAsync<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => DbContextToMock.FindAsync<TEntity>(keyValues));
            DbContextMock.Setup(m => m.FindAsync<TEntity>(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns((object[] keyValues, CancellationToken cancellationToken) => DbContextToMock.FindAsync<TEntity>(keyValues, cancellationToken));
            DbContextMock.Setup(m => m.Remove(It.IsAny<TEntity>())).Returns((TEntity entity) => DbContextToMock.Remove(entity));
            DbContextMock.Setup(m => m.Set<TEntity>()).Returns(() => DbSetMock.Object);
            DbContextMock.Setup(m => m.Update(It.IsAny<TEntity>())).Returns((TEntity entity) => DbContextToMock.Update(entity));
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