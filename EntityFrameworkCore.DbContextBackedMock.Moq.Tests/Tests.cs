﻿using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    [TestFixture]
    public class Tests {

        [Test]
        public void Add_NewEntity_Persists() {
            var builder = new DbContextMockBuilder<TestContext>();
            var mockContext = builder.GetDbContextMock();
            var context = mockContext.Object;

            var testEntity1 = new TestEntity1();
            Assert.AreEqual(default(Guid), testEntity1.Id);
            context.Set<TestEntity1>().Add(testEntity1);
            context.SaveChanges();

            Assert.AreNotEqual(default(Guid), testEntity1.Id);
            Assert.AreEqual(testEntity1, context.Find<TestEntity1>(testEntity1.Id));
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }
        
        [Test]
        public void AddWithSpecifiedDbContextAndDbSetSetUp_NewEntity_Persists() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock, false).AddSetUpDbSetFor<TestEntity1>();
            var mockContext = builder.GetDbContextMock();
            
            var context = mockContext.Object;
            var testEntity1 = new TestEntity1();
            Assert.AreEqual(default(Guid), testEntity1.Id);

            context.Set<TestEntity1>().Add(testEntity1);
            context.SaveChanges();

            Assert.AreNotEqual(default(Guid), testEntity1.Id);
            Assert.AreEqual(testEntity1, contextToMock.Find<TestEntity1>(testEntity1.Id));
            Assert.AreEqual(testEntity1, context.Find<TestEntity1>(testEntity1.Id));
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Test]
        public void FromSql_AnyStoredProcedureWithNoParameters_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);
            
            var testEntity1 = new TestEntity1();
            var list1 = new List<TestEntity1> { testEntity1 };

            builder.AddSetUpDbSetFor<TestEntity1>().WithFromSqlResult(list1.AsQueryable());

            var mockContext = builder.GetDbContextMock();
            var context = mockContext.Object;
            
            var result = context.Set<TestEntity1>().FromSql("sp_NoParams").ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            CollectionAssert.AreEquivalent(list1, result);
        }

        [Test]
        public void FromSql_SpecifiedStoredProcedureWithParameters_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);

            var testEntity1 = new TestEntity1();
            var list1 = new List<TestEntity1> { testEntity1 };
            
            var mockQueryProvider = new Mock<IQueryProvider>();
            var sqlParameter = new SqlParameter("@SomeParameter2", "Value2");
            mockQueryProvider.SetUpFromSql("sp_Specified", new List<SqlParameter> { sqlParameter }, list1.AsQueryable());
            builder.AddSetUpDbSetFor<TestEntity1>().WithQueryProviderMock(mockQueryProvider);

            var mockContext = builder.GetDbContextMock();
            var context = mockContext.Object;
            
            var result = context.Set<TestEntity1>().FromSql("[dbo].[sp_Specified] @SomeParameter1 @SomeParameter2", new SqlParameter("@someparameter2", "Value2")).ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            CollectionAssert.AreEquivalent(list1, result);
        }

        [Test]
        public void FromSql_SpecifiedStoredProcedureWithInvalidParameters_ReturnsEmptyEnumeration() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);

            var testEntity1 = new TestEntity1();
            var list1 = new List<TestEntity1> { testEntity1 };

            var sqlParameters1 = new List<SqlParameter>
            {
                new SqlParameter("@apaaplicationName", "Test Application"),
                new SqlParameter("@dffate", DateTime.Today),
                new SqlParameter("@iddds", testEntity1.Id),
                new SqlParameter("@issdsSeparator", ',')
            };

            var sqlParameters2 = new List<SqlParameter>
            {
                new SqlParameter("@applicationName", "Test Application"),
                new SqlParameter("@date", DateTime.Today),
                new SqlParameter("@ids", testEntity1.Id),
                new SqlParameter("@idsSeparator", ',')
            };

            var mockQueryProvider = new Mock<IQueryProvider>();
            mockQueryProvider.SetUpFromSql("ById", sqlParameters1, list1.AsQueryable());
            builder.AddSetUpDbSetFor<TestEntity1>().WithQueryProviderMock(mockQueryProvider);

            var mockContext = builder.GetDbContextMock();
            var context = mockContext.Object;

            var result = context.Set<TestEntity1>().FromSql("[dbo].[sp_GetLocationsById] @applicationName, @date, @ids, @idsSeparator",
                sqlParameters2.ElementAt(0), sqlParameters2.ElementAt(1), sqlParameters2.ElementAt(2), sqlParameters2.Last()).ToList();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [Test]
        public void FromSql_SpecifiedStoredProcedure_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock).AddSetUpDbSetFor<TestEntity1>();
            var mockContext = builder.GetDbContextMock();

            var context = mockContext.Object;
            var dbSet = context.Set<TestEntity1>();

            var testEntity1 = new TestEntity1();
            var testEntity2 = new TestEntity1();
            dbSet.Add(testEntity1);
            dbSet.Add(testEntity2);
            context.SaveChanges();

            //Now we have setup the data source, we can setup the query result
            var list1 = new List<TestEntity1> { testEntity1 };
            var list2 = new List<TestEntity1> { testEntity2 };

            CollectionAssert.AreNotEquivalent(list1, list2);

            var mockQueryProvider = new Mock<IQueryProvider>();
            mockQueryProvider.SetUpFromSql(list1.AsQueryable());
            mockQueryProvider.SetUpFromSql("sp_Specified", list2.AsQueryable());

            builder.WithQueryProviderMock(mockQueryProvider);

            var result1 = dbSet.FromSql("sp_NoParams").ToList();

            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.Any());
            CollectionAssert.AreEquivalent(list1, result1);

            var result2 = dbSet.FromSql("sp_Specified").ToList();

            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.Any());
            CollectionAssert.AreNotEquivalent(list1, result2);
            CollectionAssert.AreEquivalent(list2, result2);

            var sqlParameter = new SqlParameter("@SomeParameter", "SomeValue");

            mockQueryProvider.SetUpFromSql("[dbo].[sp_Specified2] @SomeParameter", new List<SqlParameter> { sqlParameter }, list2.AsQueryable());

            builder.WithQueryProviderMock(mockQueryProvider);

            var result3 = dbSet.FromSql("[dbo].[sp_Specified2] @SomeParameter", sqlParameter).ToList();

            Assert.IsNotNull(result3);
            Assert.IsTrue(result3.Any());
            CollectionAssert.AreNotEquivalent(list1, result3);
            CollectionAssert.AreEquivalent(list2, result3);
        }

        [Test]
        public void FromSql_SpecifiedStoredProcedureWithNullParameterValue_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);

            var testEntity1 = new TestEntity1();
            var list1 = new List<TestEntity1> { testEntity1 };

            var mockQueryProvider = new Mock<IQueryProvider>();
            var sqlParameter = new SqlParameter("@SomeParameter2", SqlDbType.DateTime);
            mockQueryProvider.SetUpFromSql("sp_Specified", new List<SqlParameter> { sqlParameter }, list1.AsQueryable());
            builder.AddSetUpDbSetFor<TestEntity1>().WithQueryProviderMock(mockQueryProvider);

            var mockContext = builder.GetDbContextMock();
            var context = mockContext.Object;

            var result = context.Set<TestEntity1>().FromSql("[dbo].[sp_Specified] @SomeParameter1 @SomeParameter2", new SqlParameter("@someparameter2", SqlDbType.DateTime)).ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            CollectionAssert.AreEquivalent(list1, result);
        }

        [Test]
        public void Add_NewEntity_PersistsToBothDbSetAndDbContextDbSetProperty() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock).AddSetUpForAllDbSets();

            var mockContext = builder.GetDbContextMock();
            var mockedContext = mockContext.Object;

            var list1 = new List<TestEntity1>() { new TestEntity1(), new TestEntity1() };
            mockedContext.Set<TestEntity1>().AddRange(list1);
            mockedContext.SaveChanges();

            Assert.IsTrue(mockedContext.Set<TestEntity1>().Any()); //DbSet
            Assert.IsTrue(mockedContext.TestEntities.Any()); //DbContext DbSet property
            CollectionAssert.AreEquivalent(mockedContext.Set<TestEntity1>().ToList(), mockedContext.TestEntities.ToList());
        }
    }
}