# EntityFrameworkCore.ContextBackedMock.Moq

EntityFrameworkCore.ContextBackedMock.Moq allows you to create a mock DbContext (and mock DbSets) and have it 
backed by an actual DbContext. It's basically a delegate pattern implementation where the mock for the most part
is delegating over the top of the DbContext.

If it's just a wrapper, why bother using it? There's a couple of reasons.

It's designed to work with the Microsoft InMemory provider (https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/in-memory) that is
often used for testing. The InMemory provider is great for most causes however it doesn't do everything. That's where
this library steps it. It has specific functionality to allow testing include operations involving the FromSql extension 
plus the benefits of using a mocking framework (e.g., the ability to verify method invocation). 

If your using the InMemory provider and you need to mock FromSql or want the additional coverage provided by Moq, 
this library will do the heavy lifting for you.