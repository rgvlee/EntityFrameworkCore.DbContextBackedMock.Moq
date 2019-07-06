using System;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

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
            var dbContextMock = new Mock<TDbContext>();

            dbContextMock.Setup(m => m.Add(It.IsAny<object>())).Returns((object entity) => dbContextToMock.Add(entity));
            dbContextMock.Setup(m => m.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns((object entity, CancellationToken cancellationToken) => dbContextToMock.AddAsync(entity, cancellationToken));
            dbContextMock.Setup(m => m.AddRange(It.IsAny<object[]>())).Callback((object[] entities) => dbContextToMock.AddRange(entities));
            dbContextMock.Setup(m => m.AddRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => dbContextToMock.AddRange(entities));
            dbContextMock.Setup(m => m.AddRangeAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns((object[] entities, CancellationToken cancellationToken) => dbContextToMock.AddRangeAsync(entities, cancellationToken));
            dbContextMock.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>())).Returns((IEnumerable<object> entities, CancellationToken cancellationToken) => dbContextToMock.AddRangeAsync(entities, cancellationToken));
            dbContextMock.Setup(m => m.Attach(It.IsAny<object>())).Returns((object entity) => dbContextToMock.Attach(entity));
            dbContextMock.Setup(m => m.AttachRange(It.IsAny<object[]>())).Callback((object[] entities) => dbContextToMock.AttachRange(entities));
            dbContextMock.Setup(m => m.AttachRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => dbContextToMock.AttachRange(entities));
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.ChangeDetector).Returns(((IDbContextDependencies)dbContextToMock).ChangeDetector);
            dbContextMock.Setup(m => m.ChangeTracker).Returns(dbContextToMock.ChangeTracker);
            dbContextMock.Setup(m => m.Database).Returns(dbContextToMock.Database);
            dbContextMock.Setup(m => m.Dispose()).Callback(dbContextToMock.Dispose);
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.EntityFinderFactory).Returns(((IDbContextDependencies)dbContextToMock).EntityFinderFactory);
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.EntityGraphAttacher).Returns(((IDbContextDependencies)dbContextToMock).EntityGraphAttacher);
            dbContextMock.Setup(m => m.Entry(It.IsAny<object>())).Returns((object entity) => dbContextToMock.Entry(entity));
            dbContextMock.Setup(m => m.FindAsync(It.IsAny<Type>(), It.IsAny<object[]>())).Returns((Type entityType, object[] keyValues) => dbContextToMock.FindAsync(entityType, keyValues));
            dbContextMock.Setup(m => m.FindAsync(It.IsAny<Type>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns((Type entityType, object[] keyValues, CancellationToken cancellationToken) => dbContextToMock.FindAsync(entityType, keyValues, cancellationToken));
            dbContextMock.As<IDbQueryCache>().Setup(m => m.GetOrAddQuery(It.IsAny<IDbQuerySource>(), It.IsAny<Type>())).Returns((IDbQuerySource source, Type type) => ((IDbQueryCache)dbContextToMock).GetOrAddQuery(source, type));
            dbContextMock.As<IDbSetCache>().Setup(m => m.GetOrAddSet(It.IsAny<IDbSetSource>(), It.IsAny<Type>())).Returns((IDbSetSource source, Type type) => ((IDbSetCache)dbContextToMock).GetOrAddSet(source, type));
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.InfrastructureLogger).Returns(((IDbContextDependencies)dbContextToMock).InfrastructureLogger);
            dbContextMock.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance).Returns(((IInfrastructure<IServiceProvider>)dbContextToMock).Instance);
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.Model).Returns(((IDbContextDependencies)dbContextToMock).Model);

            //public DbQuery<TQuery> Query<TQuery>() where TQuery : class {
            //    return DbContextToMock.Query<TQuery>();
            //}

            dbContextMock.As<IDbContextDependencies>().Setup(m => m.QueryProvider).Returns(((IDbContextDependencies)dbContextToMock).QueryProvider);
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.QuerySource).Returns(((IDbContextDependencies)dbContextToMock).QuerySource);
            dbContextMock.Setup(m => m.Remove(It.IsAny<object>())).Returns((object entity) => dbContextToMock.Remove(entity));
            dbContextMock.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => dbContextToMock.RemoveRange(entities));
            dbContextMock.Setup(m => m.RemoveRange(It.IsAny<object[]>())).Callback((object[] entities) => dbContextToMock.RemoveRange(entities));
            dbContextMock.As<IDbContextPoolable>().Setup(m => m.ResetState()).Callback(() => ((IDbContextPoolable)dbContextToMock).ResetState());
            dbContextMock.As<IDbContextPoolable>().Setup(m => m.Resurrect(It.IsAny<DbContextPoolConfigurationSnapshot>())).Callback((DbContextPoolConfigurationSnapshot configurationSnapshot) => ((IDbContextPoolable)dbContextToMock).Resurrect(configurationSnapshot));
            dbContextMock.Setup(m => m.SaveChanges()).Returns(() => dbContextToMock.SaveChanges());
            dbContextMock.Setup(m => m.SaveChanges(It.IsAny<bool>())).Returns((bool acceptAllChangesOnSuccess) => dbContextToMock.SaveChanges(acceptAllChangesOnSuccess));
            dbContextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns((CancellationToken cancellationToken) => dbContextToMock.SaveChangesAsync(cancellationToken));
            dbContextMock.Setup(m => m.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>())).Returns((bool acceptAllChangesOnSuccess, CancellationToken cancellationToken) => dbContextToMock.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
            dbContextMock.As<IDbContextPoolable>().Setup(m => m.SetPool(It.IsAny<IDbContextPool>())).Callback((IDbContextPool contextPool) => ((IDbContextPoolable)dbContextToMock).SetPool(contextPool));
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.SetSource).Returns(((IDbContextDependencies)dbContextToMock).SetSource);
            dbContextMock.As<IDbContextPoolable>().Setup(m => m.SnapshotConfiguration()).Returns(((IDbContextPoolable)dbContextToMock).SnapshotConfiguration());
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.StateManager).Returns(((IDbContextDependencies)dbContextToMock).StateManager);
            dbContextMock.Setup(m => m.Update(It.IsAny<object>())).Returns((object entity) => dbContextToMock.Update(entity));
            
            dbContextMock.As<IDbContextDependencies>().Setup(m => m.UpdateLogger).Returns(((IDbContextDependencies)dbContextToMock).UpdateLogger);

            dbContextMock.Setup(m => m.UpdateRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => dbContextToMock.UpdateRange(entities));
            dbContextMock.Setup(m => m.UpdateRange(It.IsAny<object[]>())).Callback((object[] entities) => dbContextToMock.UpdateRange(entities));

            return dbContextMock;
        }
    }
}