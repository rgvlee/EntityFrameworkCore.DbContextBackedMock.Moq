using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.DbContextBackedMock.Moq
{
    /// <inheritdoc cref="IAsyncEnumerable{T}" />
    [Obsolete("This package has moved to https://www.nuget.org/packages/EntityFrameworkCore.Testing.Moq/. This package will be unlisted at a later date.")]
    public class AsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public AsyncEnumerable(Expression expression) : base(expression) { }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        public AsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }
}