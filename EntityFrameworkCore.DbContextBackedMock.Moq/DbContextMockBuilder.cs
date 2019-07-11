using EntityFrameworkCore.DbContextBackedMock.Moq.Extensions;
using EntityFrameworkCore.DbContextBackedMock.Moq.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.DbContextBackedMock.Moq {
    /// <summary>
    /// A builder that creates a DbContext mock.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext to mock type.</typeparam>
    public class DbContextMockBuilder<TDbContext>
        where TDbContext : DbContext {

        private readonly TDbContext _dbContextToMock;

        private readonly Mock<TDbContext> _dbContextMock;
        
        private readonly Dictionary<Type, Mock> _mockCache;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="addSetUpForAllDbSets">If set to true all of the DbContext sets will be set up automatically.</param>
        /// <remarks>Automatically creates a new in-memory database that will be used to back the DbContext mock.
        /// Requires the <see>
        ///     <cref>TDbContext</cref>
        /// </see>
        /// type to have a DbContextOptions constructor.</remarks>
        public DbContextMockBuilder(bool addSetUpForAllDbSets = true) :
            this((TDbContext)Activator.CreateInstance(typeof(TDbContext), new DbContextOptionsBuilder<TDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), 
                addSetUpForAllDbSets) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dbContextToMock">The DbContext to mock.</param>
        /// <param name="addSetUpForAllDbSets">If set to true all of the DbContext sets will be set up automatically.</param>
        public DbContextMockBuilder(TDbContext dbContextToMock, bool addSetUpForAllDbSets = true) {
            _mockCache = new Dictionary<Type, Mock>();

            _dbContextToMock = dbContextToMock;
            
            _dbContextMock = _dbContextToMock.CreateDbContextMock();

            if (addSetUpForAllDbSets) AddSetUpForAllDbSets();
        }

        private Mock GetMockFromCache(Type key) {
            var mock = _mockCache[key];
            if (mock == null) throw new Exception($"No set up found for '<{key.Name}>'.");
            return mock;
        }

        /// <summary>
        /// Creates mocks for all of the DbContext sets.
        /// </summary>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpForAllDbSets() {

            var properties = _dbContextToMock.GetType().GetProperties().Where(p =>
                p.PropertyType.IsGenericType && //must be a generic type for the next part of the predicate
                (typeof(DbSet<>).IsAssignableFrom(p.PropertyType.GetGenericTypeDefinition())));

            foreach (var property in properties) {
                var entityType = property.PropertyType.GenericTypeArguments.First();
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable UnusedVariable
                // ReSharper disable ArrangeThisQualifier
                var expression = typeof(ExpressionHelper).GetMethods().Single(m => m.Name.Equals(nameof(ExpressionHelper.CreatePropertyExpression)) && m.GetParameters().ToList().Count == 1).MakeGenericMethod(typeof(TDbContext), property.PropertyType).Invoke(this, new []{ property });
                var builder = this.GetType().GetMethods().Single(m => m.Name.Equals(nameof(AddSetUpFor)) && m.GetParameters().ToList().Count == 1).MakeGenericMethod(entityType).Invoke(this, new []{ expression });
                // ReSharper restore ArrangeThisQualifier
                // ReSharper restore UnusedVariable
                // ReSharper restore PossibleNullReferenceException
            }

            return this;
        }

        /// <summary>
        /// Adds the mock set up for an entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="dbSetMock">The mock DbSet.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpFor<TEntity>(
            Expression<Func<TDbContext, DbSet<TEntity>>> expression, Mock<DbSet<TEntity>> dbSetMock) where TEntity : class {

            _dbContextMock.Setup(m => m.Add(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Add(entity));
            _dbContextMock.Setup(m => m.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>())).Returns((TEntity entity, CancellationToken cancellationToken) => _dbContextToMock.AddAsync(entity, cancellationToken));
            _dbContextMock.Setup(m => m.Attach(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Attach(entity));
            _dbContextMock.Setup(m => m.AttachRange(It.IsAny<object[]>())).Callback((object[] entities) => _dbContextToMock.AttachRange(entities));
            _dbContextMock.Setup(m => m.AttachRange(It.IsAny<IEnumerable<object>>())).Callback((IEnumerable<object> entities) => _dbContextToMock.AttachRange(entities));
            _dbContextMock.Setup(m => m.Entry(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Entry(entity));
            _dbContextMock.Setup(m => m.Find<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => _dbContextToMock.Find<TEntity>(keyValues));
            _dbContextMock.Setup(m => m.Find(typeof(TEntity), It.IsAny<object[]>())).Returns((Type type, object[] keyValues) => _dbContextToMock.Find(type, keyValues));
            _dbContextMock.Setup(m => m.FindAsync<TEntity>(It.IsAny<object[]>())).Returns((object[] keyValues) => _dbContextToMock.FindAsync<TEntity>(keyValues));
            _dbContextMock.Setup(m => m.FindAsync<TEntity>(It.IsAny<object[]>(), It.IsAny<CancellationToken>())).Returns((object[] keyValues, CancellationToken cancellationToken) => _dbContextToMock.FindAsync<TEntity>(keyValues, cancellationToken));
            _dbContextMock.Setup(m => m.Remove(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Remove(entity));
            _dbContextMock.Setup(expression).Returns(() => dbSetMock.Object);
            _dbContextMock.Setup(m => m.Set<TEntity>()).Returns(() => dbSetMock.Object);
            _dbContextMock.Setup(m => m.Update(It.IsAny<TEntity>())).Returns((TEntity entity) => _dbContextToMock.Update(entity));

            return this;
        }

        /// <summary>
        /// Adds the mock set up for an entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpFor<TEntity>(Expression<Func<TDbContext, DbSet<TEntity>>> expression) where TEntity : class {
            var key = expression.ReturnType.GetGenericArguments().Single();

            if (!_mockCache.ContainsKey(key)) {
                _mockCache.Add(key, _dbContextToMock.Set<TEntity>().CreateDbSetMock());
            }
            var dbSetMock = (Mock<DbSet<TEntity>>)_mockCache[key];

            AddSetUpFor(expression, dbSetMock);

            return this;
        }

        /// <summary>
        /// Adds the mock set up for a query.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="dbQueryMock">The mock DbQuery.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpFor<TQuery>(Expression<Func<TDbContext, DbQuery<TQuery>>> expression, Mock<DbQuery<TQuery>> dbQueryMock) where TQuery : class {
            _dbContextMock.Setup(expression)
                .Callback(() => ((IEnumerable<TQuery>)dbQueryMock.Object).GetEnumerator().Reset())
                .Returns(() => dbQueryMock.Object);

            _dbContextMock.Setup(m => m.Query<TQuery>())
                .Callback(() => ((IEnumerable<TQuery>) dbQueryMock.Object).GetEnumerator().Reset())
                .Returns(() => dbQueryMock.Object);
            
            return this;
        }

        /// <summary>
        /// Adds the mock set up for a query.
        /// </summary>
        /// <typeparam name="TQuery">The query type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="sequence">The sequence to use for operations on the query.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddSetUpFor<TQuery>(Expression<Func<TDbContext, DbQuery<TQuery>>> expression, IEnumerable<TQuery> sequence) where TQuery : class {
            var key = expression.ReturnType.GetGenericArguments().Single();
            
            if (!_mockCache.ContainsKey(key)) {
                _mockCache.Add(key, _dbContextToMock.Query<TQuery>().CreateDbQueryMock(sequence));
            }
            
            var dbQueryMock = (Mock<DbQuery<TQuery>>)_mockCache[key];

            AddSetUpFor(expression, dbQueryMock);
            
            return this;
        }
        
        /// <summary>
        /// Adds the specified query provider mock to the mock set up for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="queryProviderMock">The query provider mock to add.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddQueryProviderMockFor<TEntity>(Expression<Func<TDbContext, IQueryable<TEntity>>> expression, Mock<IQueryProvider> queryProviderMock)
            where TEntity : class {
            var mock = GetMockFromCache(expression.ReturnType.GetGenericArguments().Single());
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return this;
        }
        
        /// <summary>
        /// Mocks the FromSql result for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="expectedFromSqlResult">The expected FromSql result.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddFromSqlResultFor<TEntity>(Expression<Func<TDbContext, IQueryable<TEntity>>> expression, IEnumerable<TEntity> expectedFromSqlResult)
            where TEntity : class {
            var mock = GetMockFromCache(expression.ReturnType.GetGenericArguments().Single());
            var queryProviderMock = new Mock<IQueryProvider>();
            queryProviderMock.SetUpFromSql(expectedFromSqlResult);
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return this;
        }

        /// <summary>
        /// Mocks the FromSql result for invocations containing the specified sql string for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="sql">The FromSql sql string. Mock set up supports case insensitive partial matches.</param>
        /// <param name="expectedFromSqlResult">The expected FromSql result.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddFromSqlResultFor<TEntity>(Expression<Func<TDbContext, IQueryable<TEntity>>> expression, string sql, IEnumerable<TEntity> expectedFromSqlResult)
            where TEntity : class {
            var mock = GetMockFromCache(expression.ReturnType.GetGenericArguments().Single());
            var queryProviderMock = new Mock<IQueryProvider>();
            queryProviderMock.SetUpFromSql(sql, expectedFromSqlResult);
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return this;
        }

        /// <summary>
        /// Mocks the FromSql result for invocations containing the specified sql string and sql parameters for the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="expression">The DbContext property to set up.</param>
        /// <param name="sql">The FromSql sql string. Mock set up supports case insensitive partial matches.</param>
        /// <param name="sqlParameters">The FromSql sql parameters. Mock set up supports case insensitive partial sql parameter sequence matching.</param>
        /// <param name="expectedFromSqlResult">The expected FromSql result.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddFromSqlResultFor<TEntity>(Expression<Func<TDbContext, IQueryable<TEntity>>> expression, string sql, IEnumerable<SqlParameter> sqlParameters, IEnumerable<TEntity> expectedFromSqlResult)
            where TEntity : class {
            var mock = GetMockFromCache(expression.ReturnType.GetGenericArguments().Single());
            var queryProviderMock = new Mock<IQueryProvider>();
            queryProviderMock.SetUpFromSql(sql, sqlParameters, expectedFromSqlResult);
            mock.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(queryProviderMock.Object);
            return this;
        }

        /// <summary>
        /// Sets up ExecuteSqlCommand invocations containing a specified sql string and sql parameters to return a specified result. 
        /// </summary>
        /// <param name="executeSqlCommandCommandText">The ExecuteSqlCommand sql string. Mock set up supports case insensitive partial matches.</param>
        /// <param name="sqlParameters">The ExecuteSqlCommand sql parameters. Mock set up supports case insensitive partial sql parameter sequence matching.</param>
        /// <param name="expectedResult">The integer to return when ExecuteSqlCommand is invoked.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddExecuteSqlCommandResult(string executeSqlCommandCommandText, IEnumerable<SqlParameter> sqlParameters, int expectedResult) {
            //ExecuteSqlCommand creates a RawSqlCommand then ExecuteNonQuery is executed on the relational command property.
            //We need to:
            //1) Mock the relational command ExecuteNonQuery method
            //2) Mock the RawSqlCommand (doesn't implement any interfaces so we have to use a the concrete class which requires a constructor to be specified)
            //3) Mock the IRawSqlCommandBuilder build method to return our RawSqlCommand
            //4) Mock multiple the database facade GetService methods to avoid the 'Relational-specific methods can only be used when the context is using a relational database provider.' exception.
            
            var relationalCommand = new Mock<IRelationalCommand>();
            relationalCommand.Setup(m => m.ExecuteNonQuery(It.IsAny<IRelationalConnection>(), It.IsAny<IReadOnlyDictionary<string, object>>())).Returns(() => expectedResult);
            relationalCommand.Setup(m => m.ExecuteNonQueryAsync(It.IsAny<IRelationalConnection>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(expectedResult));

            var rawSqlCommand = new Mock<RawSqlCommand>(MockBehavior.Strict, relationalCommand.Object, new Dictionary<string, object>());
            rawSqlCommand.Setup(m => m.RelationalCommand).Returns(() => relationalCommand.Object);
            rawSqlCommand.Setup(m => m.ParameterValues).Returns(new Dictionary<string, object>());

            var rawSqlCommandBuilder = new Mock<IRawSqlCommandBuilder>();
            rawSqlCommandBuilder.Setup(m => m.Build(It.Is<string>(s => s.Contains(executeSqlCommandCommandText, StringComparison.CurrentCultureIgnoreCase)), It.Is<IEnumerable<object>>(
                    parameters => !sqlParameters.Except(parameters.Select(p => (SqlParameter)p), new SqlParameterParameterNameAndValueEqualityComparer()).Any()
                    )))
                .Returns(rawSqlCommand.Object)
                .Callback((string sql, IEnumerable<object> parameters) => {
                    var sb = new StringBuilder();
                    sb.Append(sql.GetType().Name);
                    sb.Append(" sql: ");
                    sb.AppendLine(sql);

                    sb.AppendLine("Parameters:");
                    foreach (var sqlParameter in parameters.Select(p => (SqlParameter) p)) {
                        sb.Append(sqlParameter.ParameterName);
                        sb.Append(": ");
                        if (sqlParameter.Value == null)
                            sb.AppendLine("null");
                        else
                            sb.AppendLine(sqlParameter.Value.ToString());
                    }

                    Console.WriteLine(sb.ToString());
                });

            var databaseFacade = new Mock<DatabaseFacade>(MockBehavior.Strict, _dbContextToMock);
            databaseFacade.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance.GetService(It.Is<Type>(t => t == typeof(IConcurrencyDetector)))).Returns(new Mock<IConcurrencyDetector>().Object);
            databaseFacade.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance.GetService(It.Is<Type>(t => t == typeof(IRawSqlCommandBuilder)))).Returns(rawSqlCommandBuilder.Object);
            databaseFacade.As<IInfrastructure<IServiceProvider>>().Setup(m => m.Instance.GetService(It.Is<Type>(t => t == typeof(IRelationalConnection)))).Returns(new Mock<IRelationalConnection>().Object);
            
            _dbContextMock.Setup(m => m.Database).Returns(databaseFacade.Object);
            return this;
        }

        /// <summary>
        /// Sets up ExecuteSqlCommand invocations containing a specified sql string to return a specified result. 
        /// </summary>
        /// <param name="executeSqlCommandCommandText">The ExecuteSqlCommand sql string. Mock set up supports case insensitive partial matches.</param>
        /// <param name="expectedResult">The integer to return when ExecuteSqlCommand is invoked.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddExecuteSqlCommandResult(string executeSqlCommandCommandText, int expectedResult) {
            return AddExecuteSqlCommandResult(executeSqlCommandCommandText, new List<SqlParameter>(), expectedResult);
        }

        /// <summary>
        /// Sets up ExecuteSqlCommand invocations to return a specified result. 
        /// </summary>
        /// <param name="expectedResult">The integer to return when ExecuteSqlCommand is invoked.</param>
        /// <returns>The DbContext mock builder.</returns>
        public DbContextMockBuilder<TDbContext> AddExecuteSqlCommandResult(int expectedResult) {
            return AddExecuteSqlCommandResult(string.Empty, new List<SqlParameter>(), expectedResult);
        }

        /// <summary>
        /// Gets the set up DbContext mock.
        /// </summary>
        /// <returns>The DbContext mock.</returns>
        public Mock<TDbContext> GetDbContextMock() {
            return _dbContextMock;
        }

        /// <summary>
        /// Gets the set up mocked DbContext.
        /// </summary>
        /// <returns>The mocked DbContext.</returns>
        public TDbContext GetMockedDbContext() {
            return _dbContextMock.Object;
        }

        /// <summary>
        /// Gets the set up DbSet mock for the specified DbContext property.
        /// </summary>
        /// <returns>The mocked DbSet.</returns>
        public Mock<DbSet<TEntity>> GetDbSetMock<TEntity>(Expression<Func<TDbContext, DbSet<TEntity>>> expression) where TEntity : class {
            var mock = ((Mock<DbSet<TEntity>>)GetMockFromCache(expression.ReturnType.GetGenericArguments().Single()));
            return mock;
        }

        /// <summary>
        /// Gets the set up mocked DbSet for the specified DbContext property.
        /// </summary>
        /// <returns>The mocked DbSet.</returns>
        public DbSet<TEntity> GetMockedDbSet<TEntity>(Expression<Func<TDbContext, DbSet<TEntity>>> expression) where TEntity : class {
            return GetDbSetMock(expression).Object;
        }

        /// <summary>
        /// Gets the set up DbQuery mock for the specified DbContext property.
        /// </summary>
        /// <returns>The mocked DbQuery.</returns>
        public Mock<DbQuery<TEntity>> GetDbQueryMock<TEntity>(Expression<Func<TDbContext, DbQuery<TEntity>>> expression) where TEntity : class {
            var mock = ((Mock<DbQuery<TEntity>>)GetMockFromCache(expression.ReturnType.GetGenericArguments().Single()));
            return mock;
        }
        
        /// <summary>
        /// Gets the set up mocked DbQuery for the specified DbContext property.
        /// </summary>
        /// <returns>The mocked DbQuery.</returns>
        public DbQuery<TEntity> GetMockedDbQuery<TEntity>(Expression<Func<TDbContext, DbQuery<TEntity>>> expression) where TEntity : class {
            return GetDbQueryMock(expression).Object;
        }
    }
}