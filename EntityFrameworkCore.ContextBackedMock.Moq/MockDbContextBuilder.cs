// ReSharper disable UnusedMember.Global

using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.ContextBackedMock.Moq {
    public class MockDbContextBuilder<TDbContext>
        where TDbContext : DbContext {

        private readonly TDbContext _dbContext;
        private readonly Mock<TDbContext> _mockDbContext;

        public MockDbContextBuilder() {
            var options = new DbContextOptionsBuilder<TDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options);
            _mockDbContext = new Mock<TDbContext>();

            foreach (var dbSetPropertyInfo in _dbContext.GetPropertyInfoForAllDbSets()) {

            }
        }

        public MockDbContextBuilder(TDbContext dbContext) {
            _dbContext = dbContext;
            _mockDbContext = new Mock<TDbContext>();
        }

        public MockDbContextBuilder<TDbContext> AddMockDbSetFor<TEntity>() where TEntity : class {
            var mockDbSet = _dbContext.Set<TEntity>().CreateMock();
            return UseMockDbSetFor(mockDbSet);
        }

        public MockDbContextBuilder<TDbContext> AddMockDbSetFor<TEntity>(DbSet<TEntity> dbSet) where TEntity : class {
            var mockDbSet = dbSet.CreateMock();
            return UseMockDbSetFor(mockDbSet);
        }
        
        public MockDbContextBuilder<TDbContext> UseMockDbSetFor<TEntity>(Mock<DbSet<TEntity>> mockDbSet) where TEntity : class {
            _mockDbContext.Setup(m => m.Set<TEntity>()).Returns(() => mockDbSet.Object);

            _mockDbContext.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContext.Add(entity));
            _mockDbContext.Setup(m => m.AddRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => _dbContext.AddRange(entities));
            _mockDbContext.Setup(m => m.AddRange(It.IsAny<object[]>())).Callback((object[] entities) => _dbContext.AddRange(entities));

            _mockDbContext.Setup(m => m.Find<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => mockDbSet.Object.Find(keyValues));
            _mockDbContext.Setup(m => m.Find(typeof(TEntity), It.IsAny<object[]>())).Returns((Type type, object[] keyValues) => mockDbSet.Object.Find(type, keyValues));

            _mockDbContext.Setup(m => m.Remove(It.IsAny<object>())).Returns((object entity) => _dbContext.Remove(entity));
            _mockDbContext.Setup(m => m.Remove(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContext.Remove(entity));
            _mockDbContext.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => _dbContext.RemoveRange(entities));
            _mockDbContext.Setup(m => m.RemoveRange(It.IsAny<object[]>())).Callback((object[] entities) => _dbContext.RemoveRange(entities));

            _mockDbContext.Setup(m => m.Update(It.IsAny<object>())).Returns((object entity) => _dbContext.Update(entity));
            _mockDbContext.Setup(m => m.Update(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContext.Update(entity));
            _mockDbContext.Setup(m => m.UpdateRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => _dbContext.UpdateRange(entities));
            _mockDbContext.Setup(m => m.UpdateRange(It.IsAny<object[]>())).Callback((object[] entities) => _dbContext.UpdateRange(entities));

            return this;
        }

        public Mock<TDbContext> BuildMock() {
            _mockDbContext.Setup(m => m.SaveChanges()).Returns(() => _dbContext.SaveChanges());
            _mockDbContext.Setup(m => m.SaveChanges(It.IsAny<bool>())).Returns((bool acceptAllChangesOnSuccess) => _dbContext.SaveChanges(acceptAllChangesOnSuccess));
            return _mockDbContext;
        }

        public TDbContext BuildMockedContext() {
            return BuildMock().Object;
        }
    }
}
