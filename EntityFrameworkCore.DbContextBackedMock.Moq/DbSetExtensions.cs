using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// Extensions for DbSets.
    /// </summary>
    public static class DbSetExtensions {
        /// <summary>
        /// Creates a DbSet mock for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="dbSet">The DbSet to mock.</param>
        /// <returns>A DbSet mock for the specified entity.</returns>
        public static Mock<DbSet<TEntity>> CreateDbSetMock<TEntity>(this DbSet<TEntity> dbSet) where TEntity : class {
            var mock = new Mock<DbSet<TEntity>>();

            mock.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Add(entity));

            mock.Setup(m => m.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>())).Returns((TEntity entity, CancellationToken cancellationToken) => dbSet.AddAsync(entity, cancellationToken));

            mock.Setup(m => m.AddRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.AddRange(entities));
            mock.Setup(m => m.AddRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.AddRange(entities));

            mock.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<TEntity>>(), It.IsAny<CancellationToken>())).Returns((IEnumerable<TEntity> entities, CancellationToken cancellationToken) => dbSet.AddRangeAsync(entities, cancellationToken));
            mock.Setup(m => m.AddRangeAsync(It.IsAny<TEntity[]>())).Returns((TEntity[] entities) => dbSet.AddRangeAsync(entities));

            mock.As<IAsyncEnumerableAccessor<TEntity>>().Setup(m => m.AsyncEnumerable).Returns(((IAsyncEnumerableAccessor<TEntity>)dbSet).AsyncEnumerable);

            mock.Setup(m => m.Attach(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Attach(entity));
            mock.Setup(m => m.AttachRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.AttachRange(entities));
            mock.Setup(m => m.AttachRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.AttachRange(entities));

            mock.As<IListSource>().Setup(m => m.ContainsListCollection).Returns(((IListSource)dbSet).ContainsListCollection);

            mock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(((IQueryable<TEntity>)dbSet).ElementType);
            mock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(((IQueryable<TEntity>)dbSet).Expression);

            mock.Setup(m => m.Find(It.IsAny<object[]>())).Returns((object[] keyValues) => dbSet.Find(keyValues));

            mock.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns((object[] keyValues) => dbSet.FindAsync(keyValues));
            mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns((object[] keyValues, CancellationToken cancellationToken) => dbSet.FindAsync(keyValues, cancellationToken));

            mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable) dbSet).GetEnumerator());
                
            mock.As<IEnumerable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable<TEntity>) dbSet).GetEnumerator());

            /*
             * System.NotSupportedException : Data binding directly to a store query is not supported. Instead populate a DbSet with data,
             * for example by calling Load on the DbSet, and then bind to local data to avoid sending a query to the database each time the
             * databound control iterates the data. For WPF bind to 'DbSet.Local.ToObservableCollection()'. For WinForms bind to
             * 'DbSet.Local.ToBindingList()'. For ASP.NET WebForms bind to 'DbSet.ToList()' or use Model Binding.
             */
            mock.As<IListSource>().Setup(m => m.GetList()).Returns(dbSet.ToList());

            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(((IQueryable<TEntity>)dbSet).Provider);
            
            mock.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance).Returns(((IInfrastructure<IServiceProvider>)dbSet).Instance);
            
            mock.Setup(m => m.Local).Returns(dbSet.Local);
            
            mock.Setup(m => m.Remove(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Remove(entity));
            mock.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.RemoveRange(entities));
            mock.Setup(m => m.RemoveRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.RemoveRange(entities));

            mock.Setup(m => m.Update(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Update(entity));
            mock.Setup(m => m.UpdateRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.UpdateRange(entities));
            mock.Setup(m => m.UpdateRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.UpdateRange(entities));

            return mock;
        }

        /// <summary>
        /// Adds the mock set up for the specified query provider mock to the specified DbSet mock.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="dbSetMock">The DbSet mock to add the additional set up to.</param>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <returns>The DbSet mock.</returns>
        public static Mock<DbSet<TEntity>> AddSetUpForProvider<TEntity>(this Mock<DbSet<TEntity>> dbSetMock, Mock<IQueryProvider> queryProviderMock)
            where TEntity : class {
            dbSetMock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return dbSetMock;
        }

        /// <summary>
        /// Adds the mock set up for the FromSql method the specified DbSet mock.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="dbSetMock">The DbSet mock to add the additional set up to.</param>
        /// <param name="expectedFromSqlResult">The sequence to return when FromSql is invoked.</param>
        /// <returns>The DbSet mock.</returns>
        public static Mock<DbSet<TEntity>> AddSetUpForMockFromSql<TEntity>(this Mock<DbSet<TEntity>> dbSetMock, IEnumerable<TEntity> expectedFromSqlResult) where TEntity : class {
            var mockQueryProvider = new Mock<IQueryProvider>();
            mockQueryProvider.SetUpFromSql(expectedFromSqlResult);
            dbSetMock.AddSetUpForProvider(mockQueryProvider);
            return dbSetMock;
        }
    }
}