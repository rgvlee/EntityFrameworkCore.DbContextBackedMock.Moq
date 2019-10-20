using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests
{
    public class TestRepository<TDbContext, TEntity> : IRepository<TEntity>
        where TDbContext : DbContext
        where TEntity : class
    {
        private readonly TDbContext _context;

        public TestRepository() { }

        public TestRepository(TDbContext context)
        {
            _context = context;
        }

        public string GetUsingStoredProcedureWithNoParametersSql => "[dbo].sp_StoredProcedureWithNoParameters";

        public string GetUsingStoredProcedureWithParametersSql => "[dbo].sp_StoredProcedureWithParameters @SomeParameter1 @SomeParameter2";

        public IEnumerable<TEntity> GetAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        public virtual TEntity GetById(Guid id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public virtual void Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public virtual IEnumerable<TEntity> GetUsingStoredProcedureWithNoParameters()
        {
            return _context.Set<TEntity>().FromSql(GetUsingStoredProcedureWithNoParametersSql);
        }

        public virtual IEnumerable<TEntity> GetUsingStoredProcedureWithParameters()
        {
            return _context.Set<TEntity>().FromSql(GetUsingStoredProcedureWithParametersSql, new SqlParameter("@SomeParameter1", "Value1"), new SqlParameter("@SomeParameter2", "Value2"));
        }
    }
}