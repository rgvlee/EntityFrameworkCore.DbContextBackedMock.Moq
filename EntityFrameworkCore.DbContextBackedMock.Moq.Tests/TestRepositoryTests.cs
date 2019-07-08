using EntityFrameworkCore.DbContextBackedMock.Moq.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    [TestFixture]
    public class TestRepositoryTests {
        [Test]
        public void GetUsingStoredProcedureWithNoParametersSql_WithMatchingFromSql_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);
            var mockContext = builder.GetDbContextMock();

            var context = mockContext.Object;
            var testEntity1 = new TestEntity1();
            var testEntity2 = new TestEntity1();
            context.Set<TestEntity1>().Add(testEntity1);
            context.Set<TestEntity1>().Add(testEntity2);
            context.SaveChanges();

            var repository = new TestRepository<TestContext, TestEntity1>(context);

            var mockQueryProvider = new Mock<IQueryProvider>();
            var list1 = new List<TestEntity1> { testEntity1 };
            var list2 = new List<TestEntity1> { testEntity2 };
            mockQueryProvider.SetUpFromSql(repository.GetUsingStoredProcedureWithNoParametersSql, list1.AsQueryable());
            var sqlParameters = new List<SqlParameter> {
                    new SqlParameter("@SomeParameter1", "SomeParameter1Value"),
                    new SqlParameter("@SomeParameter2", "SomeParameter2Value")
                };
            mockQueryProvider.SetUpFromSql(repository.GetUsingStoredProcedureWithParametersSql, sqlParameters, list2.AsQueryable());
            builder.AddQueryProviderMockFor(x => x.TestEntities, mockQueryProvider);

            var result1 = repository.GetUsingStoredProcedureWithNoParameters().ToList();

            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.Any());
            CollectionAssert.AreEquivalent(list1, result1);
            CollectionAssert.AreNotEquivalent(list1, list2);
        }

        [Test]
        public void GetUsingStoredProcedureWithParametersSql_WithMatchingFromSql_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);
            var mockContext = builder.GetDbContextMock();

            var context = mockContext.Object;
            var testEntity1 = new TestEntity1();
            var testEntity2 = new TestEntity1();
            context.Set<TestEntity1>().Add(testEntity1);
            context.Set<TestEntity1>().Add(testEntity2);
            context.SaveChanges();

            var repository = new TestRepository<TestContext, TestEntity1>(context);

            var mockQueryProvider = new Mock<IQueryProvider>();
            var list1 = new List<TestEntity1> { testEntity1 };
            var list2 = new List<TestEntity1> { testEntity2 };
            mockQueryProvider.SetUpFromSql(repository.GetUsingStoredProcedureWithNoParametersSql, list1.AsQueryable());
            var sqlParameters = new List<SqlParameter> {
                    new SqlParameter("@SomeParameter1", "Value1"),
                    new SqlParameter("@SomeParameter2", "Value2")
                };
            mockQueryProvider.SetUpFromSql(repository.GetUsingStoredProcedureWithParametersSql, sqlParameters, list2.AsQueryable());
            builder.AddQueryProviderMockFor(x => x.TestEntities, mockQueryProvider);

            var result1 = repository.GetUsingStoredProcedureWithParameters().ToList();

            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.Any());
            CollectionAssert.AreEquivalent(list2, result1);
            CollectionAssert.AreNotEquivalent(list1, list2);
        }

        [Test]
        public void GetUsingStoredProcedureWithParametersSql_WithDifferentFromSql_ReturnsEmptyResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);
            var mockContext = builder.GetDbContextMock();

            var context = mockContext.Object;
            var testEntity1 = new TestEntity1();
            var testEntity2 = new TestEntity1();
            context.Set<TestEntity1>().Add(testEntity1);
            context.Set<TestEntity1>().Add(testEntity2);
            context.SaveChanges();

            var repository = new TestRepository<TestContext, TestEntity1>(context);

            var mockQueryProvider = new Mock<IQueryProvider>();
            var list1 = new List<TestEntity1> { testEntity1 };
            var list2 = new List<TestEntity1> { testEntity2 };
            mockQueryProvider.SetUpFromSql(repository.GetUsingStoredProcedureWithNoParametersSql, list1.AsQueryable());
            var sqlParameters = new List<SqlParameter> {
                new SqlParameter("@SomeParameter1", "asdf"),
                new SqlParameter("@SomeParameter2", "1234")
            };
            mockQueryProvider.SetUpFromSql(repository.GetUsingStoredProcedureWithParametersSql, sqlParameters, list2.AsQueryable());
            builder.AddQueryProviderMockFor(x => x.TestEntities, mockQueryProvider);

            var result1 = repository.GetUsingStoredProcedureWithParameters().ToList();

            Assert.IsNotNull(result1);
            Assert.IsFalse(result1.Any());
        }

        [Test]
        public void Get_UsingManualSetUp_ReturnsExpectedResult() {
            var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            var builder = new DbContextMockBuilder<TestContext>(contextToMock);
            var mockContext = builder.GetDbContextMock();

            var list1 = new List<TestEntity1>() { new TestEntity1(), new TestEntity1() };
            foreach (var testEntity in list1) {
                contextToMock.Set<TestEntity1>().Add(testEntity);
            }
            contextToMock.SaveChanges();

            var context = mockContext.Object;

            var repository = new TestRepository<TestContext, TestEntity1>(context);

            var result1 = repository.GetAll().ToList();

            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.Any());
            CollectionAssert.AreEquivalent(result1, list1);
        }
    }
}