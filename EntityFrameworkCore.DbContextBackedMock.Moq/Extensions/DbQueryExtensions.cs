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
        /// <typeparam name="TQuery">The DbQuery query type.</typeparam>
        /// <param name="dbQuery">The DbQuery to mock.</param>
        /// <param name="rows">The mock representation of the DbQuery.</param>
        /// <returns>A DbQuery mock for the specified entity.</returns>
        public static Mock<DbQuery<TQuery>> CreateDbQueryMock<TQuery>(this DbQuery<TQuery> dbQuery, IEnumerable<TQuery> rows) where TQuery : class {
            var dbQueryMock = new Mock<DbQuery<TQuery>>();

            var queryable = rows.AsQueryable();

            dbQueryMock.As<IAsyncEnumerableAccessor<TQuery>>().Setup(m => m.AsyncEnumerable).Returns(queryable.ToAsyncEnumerable);
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbQueryMock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            dbQueryMock.As<IEnumerable<TQuery>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            //dbQueryMock.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance).Returns();
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.Provider).Returns(queryable.Provider);

            return dbQueryMock;
        }
    }
}