using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using EntityFrameworkCore.DbContextBackedMock.Moq.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbContext mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    public class DbContextMockBuilder<TDbContext>
        where TDbContext : DbContext {

        private readonly TDbContext _dbContextToMock;

        private readonly Mock<TDbContext> _dbContextMock;

        private readonly IEnumerable<PropertyInfo> _dbContextProperties;

        private readonly Dictionary<Type, object> _mockCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="addSetUpForAllSets">If set to true all of the DbContext sets will be set up automatically.</param>
        /// <param name="addSetUpForAllQueries">If set to true all of the DbContext queries will be set up automatically.</param>
        /// <remarks>Automatically creates a new in-memory database that will be used to back the DbContext mock.
        /// Requires the <see>
        ///     <cref>TDbContext</cref>
        /// </see>
        /// type to have a DbContextOptions constructor.</remarks>
        public DbContextMockBuilder(bool addSetUpForAllSets = true, bool addSetUpForAllQueries = true) :
            this((TDbContext)Activator.CreateInstance(typeof(TDbContext), new DbContextOptionsBuilder<TDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), 
                addSetUpForAllSets, 
                addSetUpForAllQueries) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        /// <param name="addSetUpForAllSets">If set to true all of the DbContext sets will be set up automatically.</param>
        /// <param name="addSetUpForAllQueries">If set to true all of the DbContext queries will be set up automatically.</param>
        public DbContextMockBuilder(TDbContext dbContextToMock, bool addSetUpForAllSets = true, bool addSetUpForAllQueries = true) {
            _mockCache = new Dictionary<Type, object>();

            _dbContextToMock = dbContextToMock;

            _dbContextProperties = _dbContextToMock.GetType().GetProperties().Where(p =>
                p.PropertyType.IsGenericType && //must be a generic type for the next part of the predicate
                (typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()) ||
                 typeof(DbQuery<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition())));

            _dbContextMock = _dbContextToMock.CreateDbContextMock();

            if (addSetUpForAllSets) AddSetUpForAllSets();
            if (addSetUpForAllQueries) AddSetUpForAllQueries();
        }
        
        private object GetOrCreateMockFor<TEntity>() where TEntity : class {
            if (!_mockCache.ContainsKey(typeof(TEntity))) {
                AddSetUpFor<TEntity>();
            }
            return _mockCache[typeof(TEntity)];
        }
        
        /// <summary>
        /// Creates mocks for all of the DbContext sets.
        /// </summary>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpForAllSets() {
            foreach (var propertyInfo in _dbContextProperties.Where(q => typeof(DbSet<>).IsAssignableFrom(q.PropertyType.GetGenericTypeDefinition()))) {
                var entityType = propertyInfo.PropertyType.GenericTypeArguments.First();
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable UnusedVariable
                // ReSharper disable ArrangeThisQualifier
                var builder = this.GetType().GetMethod(nameof(AddSetUpFor)).MakeGenericMethod(entityType).Invoke(this, null);
                // ReSharper restore ArrangeThisQualifier
                // ReSharper restore UnusedVariable
                // ReSharper restore PossibleNullReferenceException
            }

            return this;
        }

        /// <summary>
        /// Creates mocks for all of the DbContext queries.
        /// </summary>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpForAllQueries() {
            foreach (var propertyInfo in _dbContextProperties.Where(q => typeof(DbQuery<>).IsAssignableFrom(q.PropertyType.GetGenericTypeDefinition()))) {
                var entityType = propertyInfo.PropertyType.GenericTypeArguments.First();
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable UnusedVariable
                // ReSharper disable ArrangeThisQualifier
                var builder = this.GetType().GetMethod(nameof(AddSetUpFor)).MakeGenericMethod(entityType).Invoke(this, null);
                // ReSharper restore ArrangeThisQualifier
                // ReSharper restore UnusedVariable
                // ReSharper restore PossibleNullReferenceException
            }

            return this;
        }

        /// <summary>
        /// Adds the mock set up for an entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpFor<TEntity>() where TEntity : class{
            foreach (var propertyInfo in _dbContextProperties.Where(q => typeof(DbSet<>).IsAssignableFrom(q.PropertyType.GetGenericTypeDefinition()))) {
                var entityType = propertyInfo.PropertyType.GenericTypeArguments.First();
                if (entityType != typeof(TEntity)) continue;

                if (!_mockCache.ContainsKey(typeof(TEntity))) {
                    _mockCache.Add(typeof(TEntity), _dbContextToMock.Set<TEntity>().CreateDbSetMock());
                }
                var dbSetMock = (Mock<DbSet<TEntity>>)_mockCache[typeof(TEntity)];
                
                _dbContextMock.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Add(entity));
                _dbContextMock.Setup(m => m.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>())).Returns((TEntity entity, CancellationToken cancellationToken) => _dbContextToMock.AddAsync(entity, cancellationToken));
                _dbContextMock.Setup(m => m.Attach(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Attach(entity));
                _dbContextMock.Setup(m => m.AttachRange(It.IsAny<object[]>())).Callback((object[] entities) => _dbContextToMock.AttachRange(entities));
                _dbContextMock.Setup(m => m.AttachRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => _dbContextToMock.AttachRange(entities));
                _dbContextMock.Setup(m => m.Entry(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Entry(entity));
                _dbContextMock.Setup(m => m.Find<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => _dbContextToMock.Find<TEntity>(keyValues));
                _dbContextMock.Setup(m => m.Find(typeof(TEntity), It.IsAny<object[]>())).Returns((Type type, object[] keyValues) => _dbContextToMock.Find(type, keyValues));
                _dbContextMock.Setup(m => m.FindAsync<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => _dbContextToMock.FindAsync<TEntity>(keyValues));
                _dbContextMock.Setup(m => m.FindAsync<TEntity>(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns((object[] keyValues, CancellationToken cancellationToken) => _dbContextToMock.FindAsync<TEntity>(keyValues, cancellationToken));
                _dbContextMock.Setup(m => m.Remove(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Remove(entity));
                _dbContextMock.Setup(m => m.Set<TEntity>()).Returns(() => dbSetMock.Object);
                _dbContextMock.Setup(m => m.Update(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Update(entity));
                
                return this;
            }

            foreach (var propertyInfo in _dbContextProperties.Where(q => typeof(DbQuery<>).IsAssignableFrom(q.PropertyType.GetGenericTypeDefinition()))) {
                var entityType = propertyInfo.PropertyType.GenericTypeArguments.First();
                if (entityType != typeof(TEntity)) continue;

                if (!_mockCache.ContainsKey(typeof(TEntity))) {
                    _mockCache.Add(typeof(TEntity), _dbContextToMock.Query<TEntity>().CreateDbQueryMock());
                }
                var dbQueryMock = (Mock<DbQuery<TEntity>>)_mockCache[typeof(TEntity)];

                _dbContextMock.Setup(m => m.Query<TEntity>()).Returns(() => dbQueryMock.Object);

                return this;
            }

            return this;
        }

        /// <summary>
        /// Adds the specified query provider mock to the mock set up for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock to add.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddQueryProviderMockFor<TEntity>(Mock<IQueryProvider> queryProviderMock) where TEntity : class {
            if(_dbContextProperties.Any(p => (typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition())))) {
                var mock = ((Mock<DbSet<TEntity>>) GetOrCreateMockFor<TEntity>());
                mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            }
            else if (_dbContextProperties.Any(p => (typeof(DbQuery<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition())))) {
                var mock = ((Mock<DbQuery<TEntity>>)GetOrCreateMockFor<TEntity>());
                mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified expected FromSql result to the mock set up for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expectedFromSqlResult">The expected FromSql result.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddFromSqlResultFor<TEntity>(IEnumerable<TEntity> expectedFromSqlResult) where TEntity : class {
            var queryProviderMock = new Mock<IQueryProvider>();
            queryProviderMock.SetUpFromSql(expectedFromSqlResult);

            if (_dbContextProperties.Any(p => (typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition())))) {
                var mock = ((Mock<DbSet<TEntity>>)GetOrCreateMockFor<TEntity>());
                mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            }
            else if (_dbContextProperties.Any(p => (typeof(DbQuery<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition())))) {
                var mock = ((Mock<DbQuery<TEntity>>)GetOrCreateMockFor<TEntity>());
                mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            }
            return this;
        }

        /// <summary>
        /// Gets the set up DbContext mock.
        /// </summary>
        /// <returns>The DbContext mock.</returns>
        public Mock<TDbContext> GetDbContextMock() {
            return _dbContextMock;
        }

        /// <summary>
        /// Gets the set up mocked DbContext.
        /// </summary>
        /// <returns>The mocked DbContext.</returns>
        public TDbContext GetMockedDbContext() {
            return _dbContextMock.Object;
        }
        
        /// <summary>
        /// Gets the set up query mock for the specified entity.
        /// </summary>
        /// <typeparam name="TQueryable">The queryable type.</typeparam>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <returns>The query mock for the specified entity.</returns>
        public Mock<TQueryable> GetQueryableMockFor<TQueryable, TEntity>()
            where TQueryable : class, IQueryable<TEntity>
            where TEntity : class 
        {
            return (Mock<TQueryable>) GetOrCreateMockFor<TEntity>();
        }
    }
}