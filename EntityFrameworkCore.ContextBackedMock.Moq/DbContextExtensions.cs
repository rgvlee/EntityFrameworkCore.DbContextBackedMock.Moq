using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.ContextBackedMock {
    public static class DbContextExtensions {
        public static IEnumerable<PropertyInfo> GetPropertyInfoForAllDbSets(this DbContext context) {
            var properties = context.GetType().GetProperties().Where(p =>
                p.PropertyType.IsGenericType && //must be a generic type for the next part of the predicate
                typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()));
            return properties;
        }
    }
}