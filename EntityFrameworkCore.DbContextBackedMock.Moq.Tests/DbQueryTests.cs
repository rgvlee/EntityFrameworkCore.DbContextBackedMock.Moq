using System;
using EntityFrameworkCore.DbContextBackedMock.Moq.Extensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    [TestFixture]
    public class DbQueryTests {
        [Test]
        public void SetUpQuery_ToList_ReturnsEnumeration() {
            var list1 = new List<TestEntity2>() { new TestEntity2(), new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();
            builder.AddSetUpFor(x => x.TestView, list1);
            var mockedContext = builder.GetMockedDbContext();

            var result = mockedContext.Query<TestEntity2>().ToList();
            
            Assert.Multiple(() => {
                CollectionAssert.AreEquivalent(list1, result);
                CollectionAssert.AreEquivalent(result, mockedContext.TestView.ToList());
            });
        }

        [Test]
        public void SetUpQuery_MultipleOperations_ReturnsExpectedResults() {
            var list1 = new List<TestEntity2>() { new TestEntity2(), new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();
            builder.AddSetUpFor(x => x.TestView, list1);
            var mockedContext = builder.GetMockedDbContext();

            Assert.Multiple(() => {
                Assert.AreEqual(mockedContext.Query<TestEntity2>().Take(1), list1.Take(1));
                Assert.AreEqual(mockedContext.Query<TestEntity2>().Take(2), list1.Take(2));
                Assert.AreEqual(mockedContext.Query<TestEntity2>().First(), list1.First());
                Assert.AreEqual(mockedContext.Query<TestEntity2>().Last(), list1.Last());
                Assert.AreEqual(mockedContext.Query<TestEntity2>().ToList(), list1.ToList());
                Assert.AreEqual(mockedContext.Query<TestEntity2>().Where(x => x.Id != Guid.Empty).ToList(), list1.Where(x => x.Id != Guid.Empty).ToList());

                Assert.AreEqual(mockedContext.TestView.Take(1), list1.Take(1));
                Assert.AreEqual(mockedContext.TestView.Take(2), list1.Take(2));
                Assert.AreEqual(mockedContext.TestView.First(), list1.First());
                Assert.AreEqual(mockedContext.TestView.Last(), list1.Last());
                Assert.AreEqual(mockedContext.TestView.ToList(), list1.ToList());
                Assert.AreEqual(mockedContext.TestView.Where(x => x.Id != Guid.Empty).ToList(), list1.Where(x => x.Id != Guid.Empty).ToList());
            });
        }

        [Test]
        public async Task SetUpQuery_AsyncList_ReturnsEnumeration() {
            var list1 = new List<TestEntity2>() { new TestEntity2(), new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();
            builder.AddSetUpFor(x => x.TestView, list1).AddFromSqlResultFor(x => x.TestView, list1);
            var mockedContext = builder.GetMockedDbContext();

            var result = await mockedContext.Query<TestEntity2>().ToListAsync();

            Assert.Multiple(() => {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                CollectionAssert.AreEquivalent(list1, result);
            });
        }


        [Test]
        public void FromSql_AnyStoredProcedureWithNoParameters_ReturnsExpectedResult() {
            var list1 = new List<TestEntity2>() { new TestEntity2(), new TestEntity2() };

            var builder = new DbContextMockBuilder<TestContext>();
            builder.AddSetUpFor(x => x.TestView, list1).AddFromSqlResultFor(x => x.TestView, list1);
            var mockedContext = builder.GetMockedDbContext();
            
            var result = mockedContext.Query<TestEntity2>().FromSql("sp_NoParams").ToList();

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