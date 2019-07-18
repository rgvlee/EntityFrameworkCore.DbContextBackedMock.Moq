using System.Linq;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Extensions {
    /// <summary>
    /// Extensions for mocks.
    /// </summary>
    public static class MockExtensions {
        /// <summary>
        /// Sets up the provider for a queryable mock.
        /// </summary>
        /// <typeparam name="T">The queryable type.</typeparam>
        /// <param name="queryableMock">The queryable mock.</param>
        /// <param name="queryProviderMock">The query provider mock.</param>
        /// <returns>The queryable mock.</returns>
        public static Mock<IQueryable<T>> SetUpProvider<T>(this Mock<IQueryable<T>> queryableMock, Mock<IQueryProvider> queryProviderMock)
            where T : class 
        {
            queryableMock.Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return queryableMock;
        }
    }
}