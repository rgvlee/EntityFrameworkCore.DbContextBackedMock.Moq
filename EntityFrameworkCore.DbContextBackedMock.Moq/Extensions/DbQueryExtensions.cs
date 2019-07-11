using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

            dbQueryMock.As<IAsyncEnumerableAccessor<TQuery>>().Setup(m => m.AsyncEnumerable).Returns(queryableSequence.ToAsyncEnumerable); //.Callback(() => Console.WriteLine("AsyncEnumerable invoked"));
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.ElementType).Returns(queryableSequence.ElementType); //.Callback(() => Console.WriteLine("ElementType invoked"));
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.Expression).Returns(queryableSequence.Expression); //.Callback(() => Console.WriteLine("Expression invoked"));
            dbQueryMock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(queryableSequence.GetEnumerator()); //.Callback(() => Console.WriteLine("IEnumerable.GetEnumerator invoked"));

            dbQueryMock.As<IEnumerable<TQuery>>().Setup(m => m.GetEnumerator()).Returns(queryableSequence.GetEnumerator()); //.Callback(() => Console.WriteLine("IEnumerable<TQuery>.GetEnumerator invoked"));

            dbQueryMock.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance).Returns(() => ((IInfrastructure<IServiceProvider>)dbQueryMock.Object).Instance); //.Callback(() => Console.WriteLine("Instance invoked"));
            dbQueryMock.As<IQueryable<TQuery>>().Setup(m => m.Provider).Returns(queryableSequence.Provider); //.Callback(() => Console.WriteLine("Provider invoked"));

            return dbQueryMock;
        }
    }
}