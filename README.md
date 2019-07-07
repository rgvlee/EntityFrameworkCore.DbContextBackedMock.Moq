

# EntityFrameworkCore.DbContextBackedMock.Moq
__*The EntityFrameworkCore FromSql mocking library*__

EntityFrameworkCore.DbContextBackedMock.Moq allows you to create a mock DbContext (and mock DbSets) and have it backed by an actual DbContext. It's basically a delegation pattern implementation where the mock for the most part is delegating to a DbContext.

If it's just a wrapper, why bother using it? There's a couple of reasons.

It's __designed to work with the Microsoft InMemory provider__ (https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/in-memory) that is
often used for testing. The InMemory provider is great for most cases however it doesn't do everything. That's where this library steps in. It has specific functionality to allow operations involving the FromSql extension to be included in your tests, as well as all of the benefits of using a mocking framework (e.g., the ability to verify method invocation). 

If you're using the InMemory provider and you need to mock FromSql or want the additional coverage provided by Moq, this library will do the heavy lifting for you.

Consumption is via a builder.

## Download
NuGet: [https://www.nuget.org/packages/EntityFrameworkCore.DbContextBackedMock.Moq/](https://www.nuget.org/packages/EntityFrameworkCore.DbContextBackedMock.Moq/)
## Fluent interface
The builder provides a fluent interface for both DbContext and DbSet mocks. I've designed the API to be intuitive and discoverable. The examples below do touch on a bit of the available functionality.
There are actually 2 builders, 1 for the DbContext and 1 for DbSets. Once you invoke a DbSet method you'll get the DbSet builder. This is deliberate as it provides explicit entity DbSet set up scoping.

## The disclaimer
The library sets up a lot of the DbContext functionality but not all of it. I have built this based on my current needs. If you find this library useful and need additional behaviour mocked flick me a message and I'll see what I can do.

### TODO
- Add mock set up for DbContext.DbQuery
- Add mock set up for DbContext.Database.ExecuteSqlCommand()

## Example Usage
- Create the builder
- Get the db context mock
- Consume

In this example the builder automatically creates an in-memory context and sets up the mock set ups for all of the DbContext DbSets. Operations on the mock DbContext are funneled through to the in memory DbContext. You can add/update/remove on either and both will yield the same result.

__Note: automatic DbContext creation requires a DbContext constructor with a single DbContextOptions parameter__.

```
[Test]
public void Add_NewEntity_Persists() {
    var builder = new DbContextMockBuilder<TestContext>();
    var mockContext = builder.GetDbContextMock();
    var mockedContext = mockContext.Object;
    var testEntity1 = new TestEntity1();

    mockedContext.Set<TestEntity1>().Add(testEntity1);
    mockedContext.SaveChanges();

    Assert.Multiple(() => {
        Assert.AreNotEqual(default(Guid), testEntity1.Id);
        Assert.IsTrue(mockedContext.Set<TestEntity1>().Any()); //DbSet
        Assert.IsTrue(mockedContext.TestEntities.Any()); //DbContext DbSet<TEntity> property
        Assert.AreEqual(testEntity1, mockedContext.Find<TestEntity1>(testEntity1.Id));
        mockContext.Verify(m => m.SaveChanges(), Times.Once);
    });
}
```
Or if you want to provide your own DbContext and only set up a specified DbSet:
- Create the context to mock
- Create the builder providing the constructor parameters:
	- The context to mock you've just created
	- addSetUpForAllDbSets = false
- Set up the DbSet you want to mock
- Consume
```
[Test]
public void AddWithSpecifiedDbContextAndDbSetSetUp_NewEntity_Persists() {
    var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var builder = new DbContextMockBuilder<TestContext>(contextToMock, false).AddSetUpDbSetFor<TestEntity1>();
    var mockContext = builder.GetDbContextMock();
    var mockedContext = mockContext.Object;
    var testEntity1 = new TestEntity1();
            
    mockedContext.Set<TestEntity1>().Add(testEntity1);
    mockedContext.SaveChanges();

    Assert.Multiple(() => {
        Assert.AreNotEqual(default(Guid), testEntity1.Id);

        Assert.IsTrue(contextToMock.Set<TestEntity1>().Any()); //DbSet
        Assert.IsTrue(contextToMock.TestEntities.Any()); //DbContext DbSet<TEntity> property
        Assert.AreEqual(testEntity1, contextToMock.Find<TestEntity1>(testEntity1.Id));

        Assert.IsTrue(mockedContext.Set<TestEntity1>().Any()); //DbSet
        Assert.IsTrue(mockedContext.TestEntities.Any()); //DbContext DbSet<TEntity> property
        Assert.AreEqual(testEntity1, mockedContext.Find<TestEntity1>(testEntity1.Id));

        mockContext.Verify(m => m.SaveChanges(), Times.Once);
    });
}
```
The mock set up covers both Set<TEntity> and the DbContext DbSet<TEntity> property:
```
[Test]
public void Add_NewEntity_PersistsToBothDbSetAndDbContextDbSetProperty() {
    var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var builder = new DbContextMockBuilder<TestContext>(contextToMock).AddSetUpForAllDbSets();
    var mockContext = builder.GetDbContextMock();
    var mockedContext = mockContext.Object;
    var list1 = new List<TestEntity1>() { new TestEntity1(), new TestEntity1() };

    mockedContext.Set<TestEntity1>().AddRange(list1);
    mockedContext.SaveChanges();

    Assert.Multiple(() => {
        Assert.IsTrue(mockedContext.Set<TestEntity1>().Any()); //DbSet
        Assert.IsTrue(mockedContext.TestEntities.Any()); //DbContext DbSet property
        CollectionAssert.AreEquivalent(mockedContext.Set<TestEntity1>().ToList(), mockedContext.TestEntities.ToList());
    });
}
```
### Testing FromSql
The main difference here is that we need the seed data to set up query provider to return the expected result.
Create in memory DbContext/mock DbContext/generate seed data/create and set up/invoke FromSql.
In this case we didn't need to persist the seed data; it will depend on implementation of your FromSql usage (e.g., if FromSql is invoked with a repository 
and the operation invokes other DbContext/DbSet operations you may need to persist the seed data).
```
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

    Assert.Multiple(() => {
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any());
        CollectionAssert.AreEquivalent(list1, result);
    });
}
```
### Testing FromSql with SqlParameters
Expanding on the previous example, for this test we create a mock query provider and specify 
- The FromSql sql that we want to match;
- A sequence of FromSql SqlParameters

The FromSql sql set up is based on a case insensitive contains; in the example we're able to match on just the stored procedure name. The FromSql SqlParameters, if provided to the query provider mock, must be provided in the FromSql invocation for a match to occur. Again, the match is case insensitive as demonstrated below.
Only FromSql SqlParameters provided to the query provider mock will be checked. All others will be ignored so you only need to specify the bare minimum for a mock setup match.
```
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

    Assert.Multiple(() => {
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any());
        CollectionAssert.AreEquivalent(list1, result);
    });
}
```