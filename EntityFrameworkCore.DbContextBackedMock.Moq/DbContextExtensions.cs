using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// Extensions for DbContexts
    /// </summary>
    public static class DbContextExtensions {
        /// <summary>
        /// Gets the property info of all of the DbSet properties for the specified DbContext.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns>The property info of all of the DbSet properties for the specified DbContext.</returns>
        public static IEnumerable<PropertyInfo> GetPropertyInfoForAllDbSets(this DbContext dbContext) {
            var properties = dbContext.GetType().GetProperties().Where(p =>
                p.PropertyType.IsGenericType && //must be a generic type for the next part of the predicate
                typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()));
            return properties;
        }

        /// <summary>
        /// Creates a DbContext mock that delegates over the specified DbContext.
        /// </summary>
        /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        /// <returns>A DbContext mock that delegates over the specified DbContext.</returns>
        public static Mock<TDbContext> CreateDbContextMock<TDbContext>(this TDbContext dbContextToMock)
            where TDbContext : DbContext {
            var mock = new Mock<TDbContext>();
            mock.Setup(m => m.SaveChanges()).Returns(() => dbContextToMock.SaveChanges());
            mock.Setup(m => m.SaveChanges(It.IsAny<bool>())).Returns((bool acceptAllChangesOnSuccess) =>
                dbContextToMock.SaveChanges(acceptAllChangesOnSuccess));
            return mock;
        }
    }
}