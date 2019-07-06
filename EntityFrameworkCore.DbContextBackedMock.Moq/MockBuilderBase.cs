using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// The base for mock builders.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    public abstract class MockBuilderBase<TDbContext>
        where TDbContext : DbContext {

        /// <summary>
        /// The DbContext to mock.
        /// </summary>
        protected internal TDbContext DbContextToMock;

        /// <summary>
        /// The DbContext mock.
        /// </summary>
        protected internal Mock<TDbContext> DbContextMock;

        /// <summary>
        /// The DbSet mock cache.
        /// </summary>
        private readonly IDictionary<Type, object> _dbSetMockCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected MockBuilderBase() {
            _dbSetMockCache = new Dictionary<Type, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        /// <param name="dbContextMock">The DbContext mock.</param>
        protected MockBuilderBase(TDbContext dbContextToMock, Mock<TDbContext> dbContextMock) : this() {
            DbContextToMock = dbContextToMock;
            DbContextMock = dbContextMock;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mockBuilderBase">The mock builder base.</param>
        protected MockBuilderBase(MockBuilderBase<TDbContext> mockBuilderBase) : this() {
            DbContextToMock = mockBuilderBase.DbContextToMock;
            DbContextMock = mockBuilderBase.DbContextMock;
            _dbSetMockCache = mockBuilderBase._dbSetMockCache;
        }
        
        /// <summary>
        /// Gets the DbSetMockBuilder for the specified entity from the internal cache.
        /// If a DbSetMockBuilder for the specified entity was not already in the cache one will be created and added to the cache.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> GetOrCreateDbSetMockBuilderFor<TEntity>() where TEntity : class {
            if (_dbSetMockCache.ContainsKey(typeof(TEntity))) {
                return (DbSetMockBuilder<TDbContext, TEntity>)_dbSetMockCache[typeof(TEntity)];
            }

            var builder = new DbSetMockBuilder<TDbContext, TEntity>(this);
            _dbSetMockCache.Add(typeof(TEntity), builder);
            return builder;
        }

        /// <summary>
        /// Adds the DbSet mock set ups for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> AddSetUpDbSetFor<TEntity>() where TEntity : class {
            return GetOrCreateDbSetMockBuilderFor<TEntity>();
        }

        /// <summary>
        /// Creates DbSet mocks for all of the DbContext DbSet properties.
        /// </summary>
        /// <returns>The DbContext mock builder.</returns>
        public MockBuilderBase<TDbContext> AddSetUpForAllDbSets() {
            foreach (var propertyInfo in DbContextToMock.GetPropertyInfoForAllDbSets()) {
                var dbSetPropertyName = propertyInfo.Name;
                var dbSetPropertyType = propertyInfo.PropertyType;
                var entityType = propertyInfo.PropertyType.GenericTypeArguments.First();

                var builder = this.GetType().GetMethod(nameof(GetOrCreateDbSetMockBuilderFor)).MakeGenericMethod(entityType).Invoke(this, null);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified query provider mock to the DbSet mock set up for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock to add.</param>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> AddDbSetQueryProviderMockFor<TEntity>(Mock<IQueryProvider> queryProviderMock) where TEntity : class {
            var builder = GetOrCreateDbSetMockBuilderFor<TEntity>();
            builder.WithQueryProviderMock(queryProviderMock);
            return builder;
        }

        /// <summary>
        /// Adds the specified expected FromSql result to the DbSet mock set up for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="expectedFromSqlResult">The expected FromSql result.</param>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> AddDbSetFromSqlResultFor<TEntity>(IEnumerable<TEntity> expectedFromSqlResult) where TEntity : class {
            var builder = GetOrCreateDbSetMockBuilderFor<TEntity>();
            builder.WithFromSqlResult(expectedFromSqlResult);
            return builder;
        }

        /// <summary>
        /// Gets the set up DbContext mock.
        /// </summary>
        /// <returns>The DbContext mock.</returns>
        public Mock<TDbContext> GetDbContextMock() {
            return DbContextMock;
        }

        /// <summary>
        /// Gets the set up DbSet mock for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <returns>The DbSet mock for the specified entity.</returns>
        public Mock<DbSet<TEntity>> GetDbSetMockFor<TEntity>() where TEntity : class {
            return GetOrCreateDbSetMockBuilderFor<TEntity>().CreateDbSetMock();
        }
    }
}