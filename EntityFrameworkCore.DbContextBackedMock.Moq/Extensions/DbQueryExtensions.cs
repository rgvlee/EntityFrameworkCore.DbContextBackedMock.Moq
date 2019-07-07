using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Extensions {
    /// <summary>
    /// Extensions for db queries.
    /// </summary>
    public static class DbQueryExtensions {
        /// <summary>
        /// Creates a DbQuery mock for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The DbQuery entity type.</typeparam>
        /// <param name="dbQuery">The DbQuery to mock.</param>
        /// <returns>A DbQuery mock for the specified entity.</returns>
        public static Mock<DbQuery<TEntity>> CreateDbQueryMock<TEntity>(this DbQuery<TEntity> dbQuery) where TEntity : class {
            var dbQueryMock = new Mock<DbQuery<TEntity>>();
            
            dbQueryMock.As<IAsyncEnumerableAccessor<TEntity>>().Setup(m => m.AsyncEnumerable).Returns(((IAsyncEnumerableAccessor<TEntity>)dbQuery).AsyncEnumerable);
            
            dbQueryMock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(((IQueryable<TEntity>)dbQuery).ElementType);
            dbQueryMock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(((IQueryable<TEntity>)dbQuery).Expression);
            dbQueryMock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable) dbQuery).GetEnumerator());
            dbQueryMock.As<IEnumerable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable<TEntity>) dbQuery).GetEnumerator());
            dbQueryMock.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance).Returns(((IInfrastructure<IServiceProvider>)dbQuery).Instance);
            dbQueryMock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(((IQueryable<TEntity>)dbQuery).Provider);
            
            return dbQueryMock;
        }
    }
}