// ReSharper disable UnusedMember.Global

using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// Extensions for query provider mocks.
    /// </summary>
    public static class QueryProviderExtensions {
        /// <summary>
        /// Sets up DbSet FromSql invocations to return a specified sequence.
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <param name="expectedFromSqlResult">The sequence to return when FromSql is invoked.</param>
        /// <returns>The query provider mock.</returns>
        public static Mock<IQueryProvider> SetUpFromSql<TEntity>(this Mock<IQueryProvider> queryProviderMock, IEnumerable<TEntity> expectedFromSqlResult) where TEntity : class {
            queryProviderMock.Setup(p => p.CreateQuery<TEntity>(It.IsAny<MethodCallExpression>()))
                .Returns(expectedFromSqlResult.AsQueryable());

            return queryProviderMock;
        }

        /// <summary>
        /// Sets up DbSet FromSql invocations for containing a specified sql string to return a specified sequence. 
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <param name="sql">The FromSql sql string. Mock set up supports case insensitive partial matches.</param>
        /// <param name="expectedFromSqlResult">The sequence to return when FromSql is invoked.</param>
        /// <returns>The query provider mock.</returns>
        public static Mock<IQueryProvider> SetUpFromSql<TEntity>(this Mock<IQueryProvider> queryProviderMock, string sql, IEnumerable<TEntity> expectedFromSqlResult) where TEntity : class {
            //Microsoft.EntityFrameworkCore.RelationalQueryableExtensions

            //public static IQueryable<TEntity> FromSql<TEntity>(
            //  [NotNull] this IQueryable<TEntity> source,
            //  [NotParameterized] RawSqlString sql,
            //  [NotNull] params object[] parameters)

            //return source.Provider.CreateQuery<TEntity>((Expression) Expression.Call((Expression) null, RelationalQueryableExtensions.FromSqlMethodInfo.MakeGenericMethod(typeof (TEntity)), source.Expression, (Expression) Expression.Constant((object) sql), (Expression) Expression.Constant((object) parameters)));

            queryProviderMock.Setup(
                    p => p.CreateQuery<TEntity>(It.Is<MethodCallExpression>(mce => SqlMatchesMethodCallExpression(mce, sql)))
                )
                .Returns(expectedFromSqlResult.AsQueryable())
                .Callback((MethodCallExpression mce) => {
                    Console.WriteLine("FromSql inputs:");
                    Console.WriteLine(StringifyFromSqlMethodCallExpression(mce));
                });

            return queryProviderMock;
        }

        /// <summary>
        /// Sets up DbSet FromSql invocations for containing a specified sql string to return a specified sequence. 
        /// </summary>
        /// <typeparam name="TEntity">The DbSet entity type.</typeparam>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <param name="sql">The FromSql sql string. Mock set up supports case insensitive partial matches.</param>
        /// <param name="sqlParameters">The FromSql sql parameters. Mock set up supports case insensitive partial sql parameter sequence matching.</param>
        /// <param name="expectedFromSqlResult">The sequence to return when FromSql is invoked.</param>
        /// <returns>The query provider mock.</returns>
        public static Mock<IQueryProvider> SetUpFromSql<TEntity>(this Mock<IQueryProvider> queryProviderMock, string sql, IEnumerable<SqlParameter> sqlParameters, IQueryable<TEntity> expectedFromSqlResult) where TEntity : class {
            //Microsoft.EntityFrameworkCore.RelationalQueryableExtensions

            //public static IQueryable<TEntity> FromSql<TEntity>(
            //  [NotNull] this IQueryable<TEntity> source,
            //  [NotParameterized] RawSqlString sql,
            //  [NotNull] params object[] parameters)

            //return source.Provider.CreateQuery<TEntity>((Expression) Expression.Call((Expression) null, RelationalQueryableExtensions.FromSqlMethodInfo.MakeGenericMethod(typeof (TEntity)), source.Expression, (Expression) Expression.Constant((object) sql), (Expression) Expression.Constant((object) parameters)));

            queryProviderMock.Setup(
                    p => p.CreateQuery<TEntity>(It.Is<MethodCallExpression>(mce => SpecifiedParametersMatchMethodCallExpression(mce, sql, sqlParameters)))
                )
                .Returns(expectedFromSqlResult.AsQueryable())
                .Callback((MethodCallExpression mce) => {
                    Console.WriteLine("FromSql inputs:");
                    Console.WriteLine(StringifyFromSqlMethodCallExpression(mce));
                });

            return queryProviderMock;
        }

        private static bool SqlMatchesMethodCallExpression(MethodCallExpression mce, string sql) {
            var mceRawSqlString = (RawSqlString)((ConstantExpression)mce.Arguments[1]).Value;

            var result = mceRawSqlString.Format.Contains(sql, StringComparison.CurrentCultureIgnoreCase);
            if (result) return mceRawSqlString.Format.Contains(sql, StringComparison.CurrentCultureIgnoreCase);
            Console.WriteLine($"mceRawSqlString: {mceRawSqlString.Format}");
            Console.WriteLine($"sql: {sql}");

            return mceRawSqlString.Format.Contains(sql, StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool SqlParametersMatchMethodCallExpression(MethodCallExpression mce, IEnumerable<SqlParameter> sqlParameters) {
            var mceParameters = ((object[])((ConstantExpression)mce.Arguments[2]).Value);
            var mceSqlParameters = GetSqlParameters(mceParameters).ToList();

            Console.WriteLine("mceSqlParameters:");
            foreach (var parameter in mceSqlParameters) {
                Console.WriteLine($"'{parameter.ParameterName}': '{parameter.Value}'");
            }
            Console.WriteLine("sqlParameters:");
            foreach (var parameter in sqlParameters) {
                Console.WriteLine($"'{parameter.ParameterName}': '{parameter.Value}'");
            }

            return !sqlParameters.Except(mceSqlParameters, new SqlParameterParameterNameAndValueEqualityComparer()).Any();
        }

        private class SqlParameterParameterNameAndValueEqualityComparer : EqualityComparer<SqlParameter> {
            public override bool Equals(SqlParameter x, SqlParameter y) {
                var parameterNamesAreEqual = false;
                if (x.ParameterName == null && y.ParameterName == null)
                    parameterNamesAreEqual = true;
                else if (x.ParameterName != null || y.ParameterName != null)
                    parameterNamesAreEqual = x.ParameterName.Equals(y.ParameterName, StringComparison.CurrentCultureIgnoreCase);

                var valuesAreEqual = false;
                if (x.Value == null && y.Value == null)
                    valuesAreEqual = true;
                else if (x.Value != null || y.Value != null)
                    valuesAreEqual = x.Value.ToString().Equals(y.Value.ToString(), StringComparison.CurrentCultureIgnoreCase);

                return parameterNamesAreEqual && valuesAreEqual;
            }

            public override int GetHashCode(SqlParameter obj) {
                var hashCode = obj.ParameterName.ToLower().GetHashCode();
                if (obj.Value != null)
                    hashCode += obj.Value.ToString().ToLower().GetHashCode();
                return hashCode;
            }
        }

        private static bool SpecifiedParametersMatchMethodCallExpression(MethodCallExpression mce, string sql, IEnumerable<SqlParameter> sqlParameters) {
            return SqlMatchesMethodCallExpression(mce, sql) && SqlParametersMatchMethodCallExpression(mce, sqlParameters);
        }

        private static IEnumerable<SqlParameter> GetSqlParameters(object[] parameters) {
            var result = new List<SqlParameter>();

            if (!parameters.Any()) return result;

            foreach (var parameter in parameters) {
                if (parameter is SqlParameter sqlParameter) {
                    result.Add(sqlParameter);
                }
            }
            return result;
        }

        private static string StringifyFromSqlMethodCallExpression(MethodCallExpression mce) {
            var sb = new StringBuilder();

            var rawSqlString = ((RawSqlString)((ConstantExpression)mce.Arguments[1]).Value);

            sb.Append(nameof(RawSqlString));
            sb.Append(" sql: ");
            sb.AppendLine(rawSqlString.Format);

            var parameters = (object[])((ConstantExpression)mce.Arguments[2]).Value;
            if (!parameters.Any()) return sb.ToString();

            var sqlParameters = GetSqlParameters(parameters);
            sb.AppendLine("Parameters:");
            foreach (var sqlParameter in sqlParameters) {
                sb.Append(sqlParameter.ParameterName);
                sb.Append(": ");
                sb.AppendLine(sqlParameter.Value == null ? "null" : sqlParameter.Value.ToString());
            }

            return sb.ToString();
        }
    }
}