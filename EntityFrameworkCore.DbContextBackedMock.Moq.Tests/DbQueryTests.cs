using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    [TestFixture]
    public class DbQueryTests {
        [Test]
        public void SetUpQuery_Enumeration_ReturnsEnumeration() {
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
            builder.AddSetUpFor(x => x.TestView, list1).AddFromSqlResultFor(list1);
            var context = builder.GetMockedDbContext();
            
            var result = context.Query<TestEntity2>().FromSql("sp_NoParams").ToList();

            Assert.Multiple(() => {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Any());
                CollectionAssert.AreEquivalent(list1, result);
            });
        }
    }
}