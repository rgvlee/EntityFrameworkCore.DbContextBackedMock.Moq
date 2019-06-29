using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    public static class DbSetExtensions {
        public static Mock<DbSet<TEntity>> CreateMockDbSet<TEntity>(this DbSet<TEntity> dbSet) where TEntity : class {
            var mock = new Mock<DbSet<TEntity>>();

            mock.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Add(entity));
            mock.Setup(m => m.AddRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.AddRange(entities));
            mock.Setup(m => m.AddRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.AddRange(entities));

            mock.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(((IQueryable)dbSet).ElementType);
            mock.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(((IQueryable)dbSet).Expression);

            mock.Setup(m => m.Find(It.IsAny<object[]>())).Returns((object[] keyValues) => dbSet.Find(keyValues));
            
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(((IQueryable)dbSet).Provider);

            mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable) dbSet).GetEnumerator());
                
            mock.As<IEnumerable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(() => ((IEnumerable<TEntity>) dbSet).GetEnumerator());

            mock.Setup(m => m.Remove(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Remove(entity));
            mock.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.RemoveRange(entities));
            mock.Setup(m => m.RemoveRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.RemoveRange(entities));

            mock.Setup(m => m.Update(It.IsAny<TEntity>())).Returns((TEntity entity) => dbSet.Update(entity));
            mock.Setup(m => m.UpdateRange(It.IsAny<IEnumerable<TEntity>>())).Callback((IEnumerable<TEntity> entities) => dbSet.UpdateRange(entities));
            mock.Setup(m => m.UpdateRange(It.IsAny<TEntity[]>())).Callback((TEntity[] entities) => dbSet.UpdateRange(entities));

            return mock;
        }

        public static Mock<DbSet<TEntity>> SetUpProvider<TEntity>(this Mock<DbSet<TEntity>> mock, Mock<IQueryProvider> queryProviderMock)
            where TEntity : class {
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return mock;
        }

        public static Mock<DbSet<TEntity>> SetUpFromSql<TEntity>(this Mock<DbSet<TEntity>> mock, IQueryable<TEntity> fromSqlResult) where TEntity : class {
            var mockQueryProvider = new Mock<IQueryProvider>();
            mockQueryProvider.SetupFromSql(fromSqlResult);
            mock.SetUpProvider(mockQueryProvider);
            return mock;
        }
    }
}