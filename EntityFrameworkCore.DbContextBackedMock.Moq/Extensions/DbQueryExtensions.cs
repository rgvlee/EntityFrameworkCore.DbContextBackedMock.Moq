using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="sequence">The sequence to use for the DbQuery.</param>
        /// <returns>A DbQuery mock for the specified entity.</returns>
        public static Mock<DbQuery<TQuery>> CreateDbQueryMock<TQuery>(this DbQuery<TQuery> dbQuery, IEnumerable<TQuery> sequence) where TQuery : class {
            var dbQueryMock = new Mock<DbQuery<TQuery>>();

            var queryableSequence = sequence.AsQueryable();

            dbQueryMock.As<IAsyncEnumerableAccessor<TQuery>>().Setup(m => m.AsyncEnumerable).Returns(queryableSequence.ToAsyncEnumerable);
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.ElementType).Returns(queryableSequence.ElementType);
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.Expression).Returns(queryableSequence.Expression);
            dbQueryMock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(queryableSequence.GetEnumerator());
            dbQueryMock.As<IEnumerable<TQuery>>().Setup(m => m.GetEnumerator()).Returns(queryableSequence.GetEnumerator());
            //dbQueryMock.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance).Returns();
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.Provider).Returns(queryableSequence.Provider);

            return dbQueryMock;
        }
    }
}