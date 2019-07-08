using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EntityFrameworkCore.DbContextBackedMock.Moq.Tests {
    [TestFixture]
    public class ExecuteSqlCommandTests {
        [Test]
        public void Execute_SetUpUnspecifiedQuery_ReturnsExpectedResult() {
            var builder = new DbContextMockBuilder<TestContext>();
            
            var commandText = "";
            var expectedResult = 1;

            builder.AddExecuteSqlCommandResult(commandText, new List<SqlParameter>(), expectedResult);

            var mockedContext = builder.GetMockedDbContext();

            var result = mockedContext.Database.ExecuteSqlCommand("sp_NoParams");

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Execute_SetUpSpecifiedQuery_ReturnsExpectedResult() {
            var builder = new DbContextMockBuilder<TestContext>();

            var commandText = "sp_NoParams";
            var expectedResult = 1;

            builder.AddExecuteSqlCommandResult(commandText, new List<SqlParameter>(), expectedResult);

            var mockedContext = builder.GetMockedDbContext();

            var result = mockedContext.Database.ExecuteSqlCommand("sp_NoParams");

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Execute_SetUpSpecifiedQueryWithSqlParameters_ReturnsExpectedResult() {
            var builder = new DbContextMockBuilder<TestContext>();
            
            var commandText = "sp_WithParams";
            var sqlParameters = new List<SqlParameter>() {new SqlParameter("@SomeParameter2", "Value2")};
            var expectedResult = 1;

            builder.AddExecuteSqlCommandResult(commandText, sqlParameters, expectedResult);

            var mockedContext = builder.GetMockedDbContext();

            var result = mockedContext.Database.ExecuteSqlCommand("[dbo.[sp_WithParams] @SomeParameter2", sqlParameters);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Execute_NoSetUp_Fails() {
            var builder = new DbContextMockBuilder<TestContext>();
            
            var commandText = "asdf";
            var expectedResult = 1;

            builder.AddExecuteSqlCommandResult(commandText, new List<SqlParameter>(), expectedResult);

            var mockedContext = builder.GetMockedDbContext();

            Assert.Throws<NullReferenceException>(() => {
                var result = mockedContext.Database.ExecuteSqlCommand("sp_NoParams");
            });
        }
    }
}
