using System;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Helpers
{
    /// <summary>
    ///     Helper methods for expressions.
    /// </summary>
    [Obsolete("This package has moved to https://www.nuget.org/packages/EntityFrameworkCore.Testing.Moq/. This package will be unlisted at a later date.")]
    public static class ExpressionHelper
    {
        /// <summary>
        ///     Creates a property expression.
        /// </summary>
        /// <typeparam name="TParameter">The expression parameter.</typeparam>
        /// <typeparam name="TProperty">The expression property.</typeparam>
        /// <param name="propertyInfo">The property info of the property to create the expression for.</param>
        /// <returns></returns>
        public static Expression<Func<TParameter, TProperty>> CreatePropertyExpression<TParameter, TProperty>(
            PropertyInfo propertyInfo)
        {
            var parameter = Expression.Parameter(typeof(TParameter));
            return Expression.Lambda<Func<TParameter, TProperty>>(Expression.Property(parameter, propertyInfo),
                parameter);
        }
    }
}