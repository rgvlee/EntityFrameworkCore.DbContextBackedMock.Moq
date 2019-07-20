using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkCore.DbContextBackedMock.Moq.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Extensions
{
    /// <summary>
    /// Extensions for mocks.
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Sets up the provider for a DbQuery mock.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="dbQueryMock">The DbQuery mock.</param>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <returns>The DbQuery mock.</returns>
        public static Mock<DbQuery<TEntity>> SetUpProvider<TEntity>(this Mock<DbQuery<TEntity>> dbQueryMock,
            Mock<IQueryProvider> queryProviderMock)
            where TEntity : class
        {
            dbQueryMock.As<IQueryable<TEntity>>().SetUpProvider(queryProviderMock);
            return dbQueryMock;
        }

        /// <summary>
        /// Sets up the provider for a DbSet mock.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="dbSetMock">The DbSet mock.</param>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <returns>The DbSet mock.</returns>
        public static Mock<DbSet<TEntity>> SetUpProvider<TEntity>(this Mock<DbSet<TEntity>> dbSetMock,
            Mock<IQueryProvider> queryProviderMock)
            where TEntity : class
        {
            dbSetMock.As<IQueryable<TEntity>>().SetUpProvider(queryProviderMock);
            return dbSetMock;
        }

        /// <summary>
        /// Sets up the provider for a queryable mock.
        /// </summary>
        /// <typeparam name="T">The queryable type.</typeparam>
        /// <param name="queryableMock">The queryable mock.</param>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <returns>The queryable mock.</returns>
        public static Mock<IQueryable<T>> SetUpProvider<T>(this Mock<IQueryable<T>> queryableMock,
            Mock<IQueryProvider> queryProviderMock)
            where T : class
        {
            queryableMock.Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return queryableMock;
        }

        /// <summary>
        /// Sets up a query for a DbContext mock.
        /// </summary>
        /// <typeparam name="TDbContext">The DbContext type.</typeparam>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="dbContextMock">The DbContext mock.</param>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="dbQueryMock">The mock DbQuery.</param>
        /// <returns>The DbContext mock.</returns>
        public static Mock<TDbContext> SetUpDbQueryFor<TDbContext, TQuery>(this Mock<TDbContext> dbContextMock,
            Expression<Func<TDbContext, DbQuery<TQuery>>> expression,
            Mock<DbQuery<TQuery>> dbQueryMock)
            where TDbContext : DbContext
            where TQuery : class
        {
            //var properties = typeof(TDbContext).GetProperties().Where(p =>
            //    p.PropertyType.IsGenericType && //must be a generic type for the next part of the predicate
            //    typeof(DbQuery<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()));

            //var property = properties.Single(p => p.PropertyType.GenericTypeArguments.Single() == typeof(TQuery));

            //var expression = ExpressionHelper.CreatePropertyExpression<TDbContext, DbQuery<TQuery>>(property);

            dbContextMock.Setup(expression)
                .Callback(() => ((IEnumerable<TQuery>) dbQueryMock.Object).GetEnumerator().Reset())
                .Returns(() => dbQueryMock.Object);

            dbContextMock.Setup(m => m.Query<TQuery>())
                .Callback(() => ((IEnumerable<TQuery>) dbQueryMock.Object).GetEnumerator().Reset())
                .Returns(() => dbQueryMock.Object);

            return dbContextMock;
        }

        /// <summary>
        /// Sets up a query for a DbContext mock.
        /// </summary>
        /// <typeparam name="TDbContext">The DbContext type.</typeparam>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="dbContextMock">The DbContext mock.</param>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="sequence">The sequence to use for the DbQuery.</param>
        /// <returns>The DbContext mock.</returns>
        public static Mock<TDbContext> SetUpDbQueryFor<TDbContext, TQuery>(this Mock<TDbContext> dbContextMock,
            Expression<Func<TDbContext, DbQuery<TQuery>>> expression,
            IEnumerable<TQuery> sequence)
            where TDbContext : DbContext
            where TQuery : class
        {
            var dbQueryMock = DbQueryHelper.CreateDbQueryMock(sequence);
            dbContextMock.SetUpDbQueryFor(expression, dbQueryMock);
            return dbContextMock;
        }
    }
}