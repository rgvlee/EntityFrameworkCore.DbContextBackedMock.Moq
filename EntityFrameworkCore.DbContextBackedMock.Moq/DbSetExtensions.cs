using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            mock.Setup(m => m.AddRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.AddRange(entities));
            mock.Setup(m => m.AddRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.AddRange(entities));

            mock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(((IQueryable)dbSet).ElementType);
            mock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(((IQueryable)dbSet).Expression);

            mock.Setup(m => m.Find(It.IsAny<object[]>())).Returns((object[] keyValues) => dbSet.Find(keyValues));
            
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(((IQueryable)dbSet).Provider);

            mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable) dbSet).GetEnumerator());
                
            mock.As<IEnumerable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable<TEntity>) dbSet).GetEnumerator());

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