// ReSharper disable UnusedMember.Global

using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.ContextBackedMock.Moq {
    public static class QueryProviderExtensions {
        public static Mock<IQueryProvider> AddFromSqlResult<TEntity>(this Mock<IQueryProvider> queryProviderMock, IQueryable<TEntity> expectedFromSqlResult) where TEntity : class {
            queryProviderMock.Setup(p => p.CreateQuery<TEntity>(It.IsAny<MethodCallExpression>()))
                .Returns(expectedFromSqlResult);

            return queryProviderMock;
        }

        public static Mock<IQueryProvider> AddFromSqlResult<TEntity>(this Mock<IQueryProvider> queryProviderMock, string sql, IQueryable<TEntity> expectedFromSqlResult) where TEntity : class {
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

        public static Mock<IQueryProvider> AddFromSqlResult<TEntity>(this Mock<IQueryProvider> queryProviderMock, string sql, IEnumerable<SqlParameter> sqlParameters, IQueryable<TEntity> expectedFromSqlResult) where TEntity : class {
            //Microsoft.EntityFrameworkCore.RelationalQueryableExtensions

            //public static IQueryable<TEntity> FromSql<TEntity>(
            //  [NotNull] this IQueryable<TEntity> source,
            //  [NotParameterized] RawSqlString sql,
            //  [NotNull] params object[] parameters)

            //return source.Provider.CreateQuery<TEntity>((Expression) Expression.Call((Expression) null, RelationalQueryableExtensions.FromSqlMethodInfo.MakeGenericMethod(typeof (TEntity)), source.Expression, (Expression) Expression.Constant((object) sql), (Expression) Expression.Constant((object) parameters)));
            
            queryProviderMock.Setup(
                    p => p.CreateQuery<TEntity>(It.Is<MethodCallExpression>(mce => ParametersMatchMethodCallExpression(mce, sql, sqlParameters)))
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
            return mceRawSqlString.Format.Equals(sql, StringComparison.CurrentCultureIgnoreCase);

        }
        private static bool ParametersMatchMethodCallExpression(MethodCallExpression mce, string sql, IEnumerable<SqlParameter> sqlParameters) {
            var mceParameters = ((object[])((ConstantExpression)mce.Arguments[2]).Value);
            var mceSqlParameters = GetSqlParameters(mceParameters).ToList();
            return SqlMatchesMethodCallExpression(mce, sql) && sqlParameters.IsEquivalentTo(mceSqlParameters, new SqlParameterParameterNameAndValueEqualityComparer());
        }

        private class SqlParameterParameterNameAndValueEqualityComparer : EqualityComparer<SqlParameter>, IEqualityComparer<SqlParameter> {
            public override bool Equals(SqlParameter x, SqlParameter y) {
                return x.ParameterName.Equals(y.ParameterName, StringComparison.CurrentCultureIgnoreCase) &&
                       x.Value.ToString().Equals(y.Value.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }

            public override int GetHashCode(SqlParameter obj) {
                return obj.ParameterName.GetHashCode() + obj.Value.GetHashCode();
            }
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
                sb.AppendLine(sqlParameter.Value.ToString());
            }

            return sb.ToString();
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
    }
}