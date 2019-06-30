using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    public static class DbContextExtensions {
        public static IEnumerable<PropertyInfo> GetPropertyInfoForAllDbSets(this DbContext context) {
            var properties = context.GetType().GetProperties().Where(p =>
                p.PropertyType.IsGenericType && //must be a generic type for the next part of the predicate
                typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition()));
            return properties;
        }

        public static Mock<TDbContext> CreateMockDbContext<TDbContext>(this TDbContext dbContext)
            where TDbContext : DbContext {
            var mock = new Mock<TDbContext>();
            mock.Setup(m => m.SaveChanges()).Returns(() => dbContext.SaveChanges());
            mock.Setup(m => m.SaveChanges(It.IsAny<bool>())).Returns((bool acceptAllChangesOnSuccess) =>
                dbContext.SaveChanges(acceptAllChangesOnSuccess));
            return mock;
        }

        public static Mock<TDbContext> SetUpDbSet<TDbContext, TEntity>(this Mock<TDbContext> mockDbContext,
            TDbContext dbContext)
            where TDbContext : DbContext
            where TEntity : class {

            return mockDbContext.SetUpDbSet(dbContext, dbContext.Set<TEntity>().CreateMockDbSet());
        }

        public static Mock<TDbContext> SetUpDbSet<TDbContext, TEntity>(this Mock<TDbContext> mockDbContext,
            TDbContext dbContext, DbSet<TEntity> dbSet)
            where TDbContext : DbContext
            where TEntity : class {

            return mockDbContext.SetUpDbSet(dbContext, dbSet.CreateMockDbSet());
        }

        //public static Mock<TDbContext> SetUpAllDbSets<TDbContext>(this Mock<TDbContext> mockDbContext, TDbContext dbContext)
        //    where TDbContext : DbContext {

        //    foreach (var propertyInfo in dbContext.GetPropertyInfoForAllDbSets()) {
        //        var dbSetPropertyName = propertyInfo.Name;
        //        var dbSetPropertyType = propertyInfo.PropertyType;
        //        var entityType = propertyInfo.PropertyType.GenericTypeArguments.First();
        //    }

        //    return mockDbContext;
        //}

        public static Mock<TDbContext> SetUpDbSet<TDbContext, TEntity>(this Mock<TDbContext> mockDbContext,
            TDbContext dbContext, Mock<DbSet<TEntity>> mockDbSet)
            where TDbContext : DbContext
            where TEntity : class {
            mockDbContext.Setup(m => m.Set<TEntity>()).Returns(() => mockDbSet.Object);

            foreach (var dbSetPropertyInfo in dbContext.GetPropertyInfoForAllDbSets()) { }

            mockDbContext.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => dbContext.Add(entity));
            mockDbContext.Setup(m => m.AddRange(It.IsAny<IEnumerable<object>>()))
                .Callback((IEnumerable<object> entities) => dbContext.AddRange(entities));
            mockDbContext.Setup(m => m.AddRange(It.IsAny<object[]>()))
                .Callback((object[] entities) => dbContext.AddRange(entities));

            mockDbContext.Setup(m => m.Find<TEntity>(It.IsAny<object[]>()))
                .Returns((object[] keyValues) => mockDbSet.Object.Find(keyValues));
            mockDbContext.Setup(m => m.Find(typeof(TEntity), It.IsAny<object[]>()))
                .Returns((Type type, object[] keyValues) => mockDbSet.Object.Find(type, keyValues));

            mockDbContext.Setup(m => m.Remove(It.IsAny<object>())).Returns((object entity) => dbContext.Remove(entity));
            mockDbContext.Setup(m => m.Remove(It.IsAny<TEntity>()))
                .Returns((TEntity entity) => dbContext.Remove(entity));
            mockDbContext.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<object>>()))
                .Callback((IEnumerable<object> entities) => dbContext.RemoveRange(entities));
            mockDbContext.Setup(m => m.RemoveRange(It.IsAny<object[]>()))
                .Callback((object[] entities) => dbContext.RemoveRange(entities));

            mockDbContext.Setup(m => m.Update(It.IsAny<object>())).Returns((object entity) => dbContext.Update(entity));
            mockDbContext.Setup(m => m.Update(It.IsAny<TEntity>()))
                .Returns((TEntity entity) => dbContext.Update(entity));
            mockDbContext.Setup(m => m.UpdateRange(It.IsAny<IEnumerable<object>>()))
                .Callback((IEnumerable<object> entities) => dbContext.UpdateRange(entities));
            mockDbContext.Setup(m => m.UpdateRange(It.IsAny<object[]>()))
                .Callback((object[] entities) => dbContext.UpdateRange(entities));

            return mockDbContext;
        }
    }
}