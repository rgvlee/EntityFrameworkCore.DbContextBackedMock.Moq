using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFrameworkCore.ContextBackedMock.Moq.Tests {
    [TestFixture]
    public class MockDbContextBuilderTests {
        [Test]
        public void Add_NewEntity_Persists() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var builder = new MockDbContextBuilder<TestContext>(contextToMock);
            builder.AddMockDbSetFor<TestEntity1>();
            var mockContext = builder.BuildMock();

            var context = mockContext.Object;
            var testEntity1 = new TestEntity1();
            Assert.AreEqual(default(Guid), testEntity1.Id);

            context.Set<TestEntity1>().Add(testEntity1);
            context.SaveChanges();
            Assert.AreNotEqual(default(Guid), testEntity1.Id);

            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Test]
        public void FromSql_StoredProcedure_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var builder = new MockDbContextBuilder<TestContext>(contextToMock);
            var mockDbSet = contextToMock.Set<TestEntity1>().CreateMock();

            builder.UseMockDbSetFor(mockDbSet);
            var mockContext = builder.BuildMock();

            var context = mockContext.Object;
            var dbSet = context.Set<TestEntity1>();

            var testEntity1 = new TestEntity1();
            dbSet.Add(testEntity1);
            context.SaveChanges();

            //Now we have setup the data source, we can setup the query result
            var list = new List<TestEntity1> { testEntity1 };
            mockDbSet.SetUpFromSql(list.AsQueryable());

            var result = dbSet.FromSql("sp_NoParams");

            //What am I going to get back?
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.AreSame(testEntity1, result.First());
        }

        [Test]
        public void FromSql_SpecifiedStoredProcedure_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            var builder = new MockDbContextBuilder<TestContext>(contextToMock);
            var mockDbSet = contextToMock.Set<TestEntity1>().CreateMock();

            builder.UseMockDbSetFor(mockDbSet);
            var mockContext = builder.BuildMock();

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
            mockQueryProvider.AddFromSqlResult(list1.AsQueryable());
            mockQueryProvider.AddFromSqlResult("sp_Specified", list2.AsQueryable());
            mockDbSet.SetUpProvider(mockQueryProvider);

            var result1 = dbSet.FromSql("sp_NoParams").ToList();

            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.Any());
            CollectionAssert.AreEquivalent(list1, result1);

            var result2 = dbSet.FromSql("sp_Specified").ToList();

            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.Any());
            CollectionAssert.AreNotEquivalent(list1, result2);
            CollectionAssert.AreEquivalent(list2, result2);

            var sqlParameter = new SqlParameter("SomeParameter", "SomeValue");

            mockQueryProvider.AddFromSqlResult("[dbo].[sp_Specified2] @SomeParameter", new List<SqlParameter> { sqlParameter }, list2.AsQueryable());
            mockDbSet.SetUpProvider(mockQueryProvider);

            var result3 = dbSet.FromSql("[dbo].[sp_Specified2] @SomeParameter", sqlParameter).ToList();

            Assert.IsNotNull(result3);
            Assert.IsTrue(result3.Any());
            CollectionAssert.AreNotEquivalent(list1, result3);
            CollectionAssert.AreEquivalent(list2, result3);
        }
    }
}