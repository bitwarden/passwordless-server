using System.Collections.Immutable;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Common.Backup;
using Passwordless.Service.Models;
using Passwordless.Service.Storage.Ef;

namespace Passwordless.Service.Tests.Backup;

public class TsvSerializerTests
{
    private readonly DbContextOptions<DbGlobalContext> _contextOptions;
    public TsvSerializerTests()
    {
        _contextOptions = new DbContextOptionsBuilder<DbGlobalContext>()
            .UseInMemoryDatabase("BloggingControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }
    
    [Fact]
    public void Deserialize_WhenDataIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sp = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<CsvBackupSerializer>>();
        var serializer = new CsvBackupSerializer(sp.Object, logger.Object);
        var db = new DbGlobalInMemoryContext(_contextOptions);
        var f = new Fixture();
        var appFeatures = f.Build<AppFeature>()
            .Without(x => x.Authenticators)
            .Without(x => x.Application)
            .CreateMany(3);
        

        // Act
        var actual = serializer.Serialize(appFeatures.ToImmutableList());

        // Assert
        actual.Should().NotBeNullOrEmpty();
    }
    
    public class DbGlobalInMemoryContext : DbGlobalContext
    {
        public DbGlobalInMemoryContext(DbContextOptions<DbGlobalContext> options) : base(options)
        {
        }
    }
}