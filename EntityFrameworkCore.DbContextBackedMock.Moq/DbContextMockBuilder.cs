using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbContext mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    public class DbContextMockBuilder<TDbContext> 
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
        /// Constructor.
        /// </summary>
        /// <remarks>Automatically creates a new in-memory database that will be used to back the DbContext mock.
        /// Requires the <see>
        ///     <cref>TDbContext</cref>
        /// </see>
        /// type to have a DbContextOptions constructor.</remarks>
        public DbContextMockBuilder() {
            var options = new DbContextOptionsBuilder<TDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            DbContextToMock = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options);
            DbContextMock = DbContextToMock.CreateDbContextMock();
            _dbSetMocks = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        public DbContextMockBuilder(TDbContext dbContextToMock) {
            DbContextToMock = dbContextToMock;
            DbContextMock = DbContextToMock.CreateDbContextMock();
            _dbSetMocks = new Dictionary<Type, object>();
        }

        private readonly IDictionary<Type, object> _dbSetMocks;

        private DbSetMockBuilder<TDbContext, TEntity> CreateOrGetDbSetMockBuilderFor<TEntity>() where TEntity : class {
            if (_dbSetMocks.ContainsKey(typeof(TEntity))) {
                return (DbSetMockBuilder<TDbContext, TEntity>)_dbSetMocks[typeof(TEntity)];
            }

            var builder = new DbSetMockBuilder<TDbContext, TEntity>(DbContextToMock, DbContextMock);
            _dbSetMocks.Add(typeof(TEntity), builder);
            return builder;
        }

        /// <summary>
        /// Adds the DbSet mock set ups for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> AddSetUpDbSetFor<TEntity>() where TEntity : class {
            return CreateOrGetDbSetMockBuilderFor<TEntity>();
        }

        /// <summary>
        /// Adds the specified query provider mock to the DbSet mock set up for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock to add.</param>
        /// <returns>The DbSet mock builder.</returns>
        public DbSetMockBuilder<TDbContext, TEntity> AddDbSetQueryProviderMockFor<TEntity>(Mock<IQueryProvider> queryProviderMock) where TEntity : class {
            var builder = CreateOrGetDbSetMockBuilderFor<TEntity>();
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
            var builder = CreateOrGetDbSetMockBuilderFor<TEntity>();
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
            return CreateOrGetDbSetMockBuilderFor<TEntity>().CreateDbSetMock();
        }
    }
}