using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using EntityFrameworkCore.DbContextBackedMock.Moq.Extensions;
using Moq;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    [TestFixture]
    public class DbQueryTests {
        [Test]
        public void SetUpQuery_ReturnsEnumeration() {
            var list1 = new List<TestEntity2>() { new TestEntity2(), new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();
            builder.AddSetUpFor(x => x.TestView, list1);
            var mockedContext = builder.GetMockedDbContext();
            
            Assert.Multiple(() => {
                CollectionAssert.AreEquivalent(list1, mockedContext.Query<TestEntity2>().ToList());
                CollectionAssert.AreEquivalent(mockedContext.Query<TestEntity2>().ToList(), mockedContext.TestView.ToList());
            });
        }

        [Test]
        public void FromSql_AnyStoredProcedureWithNoParameters_ReturnsExpectedResult() {
            var list1 = new List<TestEntity2>() { new TestEntity2(), new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();
            builder.AddSetUpFor(x => x.TestView, list1).AddFromSqlResultFor(x => x.TestView, list1);
            var context = builder.GetMockedDbContext();
            
            var result = context.Query<TestEntity2>().FromSql("sp_NoParams").ToList();

            Assert.Multiple(() => {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                CollectionAssert.AreEquivalent(list1, result);
            });
        }

        [Test]
        public void FromSql_SpecifiedStoredProcedureWithParameters_ReturnsExpectedResult() {
            var list1 = new List<TestEntity2> { new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();

            var mockQueryProvider = new Mock<IQueryProvider>();
            var sqlParameter = new SqlParameter("@SomeParameter2", "Value2");
            mockQueryProvider.SetUpFromSql("sp_Specified", new List<SqlParameter> { sqlParameter }, list1);
            builder.AddSetUpFor(x => x.TestView, list1).AddQueryProviderMockFor(x => x.TestView, mockQueryProvider);
            
            var context = builder.GetMockedDbContext();

            var result = context.Query<TestEntity2>().FromSql("[dbo].[sp_Specified] @SomeParameter1 @SomeParameter2", new SqlParameter("@someparameter2", "Value2")).ToList();

            Assert.Multiple(() => {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                CollectionAssert.AreEquivalent(list1, result);
            });
        }
    }
}