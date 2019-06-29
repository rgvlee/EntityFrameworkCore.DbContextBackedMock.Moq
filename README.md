# EntityFrameworkCore.DbContextBackedMock.Moq

EntityFrameworkCore.DbContextBackedMock.Moq allows you to create a mock DbContext (and mock DbSets) and have it 
backed by an actual DbContext. It's basically a delegate pattern implementation where the mock for the most part
is delegating over the top of the DbContext.

If it's just a wrapper, why bother using it? There's a couple of reasons.

It's designed to work with the Microsoft InMemory provider (https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/in-memory) that is
often used for testing. The InMemory provider is great for most cases however it doesn't do everything. That's where
this library steps in. It has specific functionality to allow operations involving the FromSql extension to be included
in your tests, as well as all of the benefits of using a mocking framework (e.g., the ability to verify method invocation). 

If your using the InMemory provider and you need to mock FromSql or want the additional coverage provided by Moq, 
this library will do the heavy lifting for you.

## Example Usage

- Create an in memory DbContext
- Create the mock DbContext
- Create the mock DbSet and add it to the mock DbContext
- Consume

Operations on the mock DbContext are funnelled through to the in memory DbContext. You can add/update/remove on either and both will yield the same result.

```
[Test]
public void Add_NewEntity_Persists() {
    var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var mockContext = contextToMock.CreateMockDbContext();
    var mockDbSet = contextToMock.Set<TestEntity1>().CreateMockDbSet();
    mockContext.AddSetupForDbSet(contextToMock, mockDbSet);

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
    var mockContext = contextToMock.CreateMockDbContext();

    var testEntity1 = new TestEntity1();
    var list1 = new List<TestEntity1> { testEntity1 };

    var mockDbSet = contextToMock.Set<TestEntity1>().CreateMockDbSet();
    mockDbSet.SetUpFromSql(list1.AsQueryable());
    mockContext.AddSetupForDbSet(contextToMock, mockDbSet);

    var context = mockContext.Object;
                
    var result = context.Set<TestEntity1>().FromSql("sp_NoParams").ToList();
	    
    Assert.IsNotNull(result);
    Assert.IsTrue(result.Any());
    CollectionAssert.AreEquivalent(list1, result);
}
```

### Testing FromSql with SqlParameters

Expanding on the previous example, for this test we create a mock query provider and specify 
- The FromSql sql that we want to match;
- A sequence of FromSql SqlParameters

The FromSql sql set up is based on a case insensitive contains; in the example we're able to match on just the stored procedure name.
The FromSql SqlParameters, if provided to the query provider mock, must be provided in the FromSql invocation for a match to occur. Again, the match is case insensitive as demonstrated below.
Only FromSql SqlParameters provided to the query provider mock will be checked. All others will be ignored so you only need to specify the bare minimum for a mock setup match.

```
[Test]
public void FromSql_SpecifiedStoredProcedureWithParameters_ReturnsExpectedResult() {
    var contextToMock = new TestContext(new DbContextOptionsBuilder<TestContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    var mockContext = contextToMock.CreateMockDbContext();

    var testEntity1 = new TestEntity1();
    var list1 = new List<TestEntity1> { testEntity1 };

    var mockDbSet = contextToMock.Set<TestEntity1>().CreateMockDbSet();

    var mockQueryProvider = new Mock<IQueryProvider>();
    var sqlParameter = new SqlParameter("SomeParameter2", "Value2");
    mockQueryProvider.SetUpFromSql("sp_Specified", new List<SqlParameter> { sqlParameter }, list1.AsQueryable());
    mockDbSet.SetUpProvider(mockQueryProvider);

    mockContext.AddSetupForDbSet(contextToMock, mockDbSet);

    var context = mockContext.Object;
            
    var result = context.Set<TestEntity1>().FromSql("[dbo].[sp_Specified] @SomeParameter1 @SomeParameter2", new SqlParameter("someparameter2", "Value2")).ToList();

    Assert.IsNotNull(result);
    Assert.IsTrue(result.Any());
    CollectionAssert.AreEquivalent(list1, result);
}
```

## The disclaimer

The library sets up a lot of the DbContext functionality, but not all of it. I have built this based on my current needs. If you find this library useful and need additional behaviour mocked, flick me a message and I'll see what I can do.
