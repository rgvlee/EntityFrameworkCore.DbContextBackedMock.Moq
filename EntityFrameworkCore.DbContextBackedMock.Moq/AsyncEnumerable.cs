using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <inheritdoc cref="IAsyncEnumerable{T}" />
    public class AsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T> {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public AsyncEnumerable(Expression expression) : base(expression) { }

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetEnumerator() => new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}