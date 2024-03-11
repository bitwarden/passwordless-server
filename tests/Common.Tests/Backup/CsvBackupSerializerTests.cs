using System.Collections.Immutable;
using System.Text;
using System.Text.Unicode;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Common.Backup;
using Passwordless.Common.Backup.Mapping;
using Passwordless.Common.Tests.Backup.DataFactory;

namespace Passwordless.Common.Tests.Backup;

public class CsvBackupSerializerTests
{
    private readonly DbBackupContext _dbContext;
    private readonly Mock<ILogger<CsvBackupSerializer>> _loggerMock = new();
    private readonly CsvBackupSerializer _sut;

    public CsvBackupSerializerTests()
    {
        _dbContext = new DbBackupContext(new DbContextOptionsBuilder<DbBackupContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        IServiceProvider serviceProvider = new ServiceCollection()
            .AddScoped<DbBackupContext>(_ => _dbContext)
            .AddScoped<ClassMap<Person>, EntityFrameworkMap<Person, DbBackupContext>>()
            .AddScoped<ClassMap<Car>, EntityFrameworkMap<Car, DbBackupContext>>()
            .BuildServiceProvider();

        _sut = new CsvBackupSerializer(serviceProvider, _loggerMock.Object);
    }

    [Fact]
    public void Serialize_WhenSerializingCars_ReturnsExpectedResultWith3Columns()
    {
        // Arrange
        var person1 = new Person { Id = 1, Name = "Ford Person" };
        var person2 = new Person { Id = 2, Name = "Chevy Person" };

        var car1 = new Car { Id = 1, Make = "Ford", OwnerId = person1.Id };
        var car2 = new Car { Id = 2, Make = "Chevy", OwnerId = person2.Id };

        _dbContext.People.AddRange(person1, person2);
        _dbContext.Cars.AddRange(car1, car2);
        _dbContext.SaveChanges();

        var cars = _dbContext.Cars.ToImmutableList();

        // Act
        var actual = _sut.Serialize(cars);

        // Assert
        Assert.Equal("Id,Make,OwnerId\r\n1,Ford,1\r\n2,Chevy,2\r\n"u8.ToArray(), actual);
    }

    [Fact]
    public void Serialize_WhenSerializingCars_ReturnsExpectedDeserializedResult()
    {
        // Arrange
        var person1 = new Person { Id = 1, Name = "Ford Person" };
        var person2 = new Person { Id = 2, Name = "Chevy Person" };

        var car1 = new Car { Id = 1, Make = "Ford,", OwnerId = person1.Id };
        var car2 = new Car { Id = 2, Make = "Chevy", OwnerId = person2.Id };

        _dbContext.People.AddRange(person1, person2);
        _dbContext.Cars.AddRange(car1, car2);
        _dbContext.SaveChanges();

        var cars = _dbContext.Cars.ToImmutableList();

        // Act
        var temp = _sut.Serialize(cars);
        var actual = _sut.Deserialize<Car>(temp).ToList();

        // Assert
        Assert.Equal(2, actual.Count);
        Assert.Equal(1, actual.First().Id);
        Assert.Equal(1, actual.First().OwnerId);
        Assert.Equal("Ford,", actual.First().Make);
        Assert.Equal(2, actual.Last().Id);
        Assert.Equal(2, actual.Last().OwnerId);
        Assert.Equal("Chevy", actual.Last().Make);
    }

    [Fact]
    public void Serialize_WhenSerializingPeople_ReturnsExpectedResultWith2Columns()
    {
        // Arrange
        var person1 = new Person { Id = 1, Name = "Ford Person" };
        var person2 = new Person { Id = 2, Name = "Chevy Person" };

        var car1 = new Car { Id = 1, Make = "Ford", OwnerId = person1.Id };
        var car2 = new Car { Id = 2, Make = "Chevy", OwnerId = person2.Id };

        _dbContext.People.AddRange(person1, person2);
        _dbContext.Cars.AddRange(car1, car2);
        _dbContext.SaveChanges();

        var people = _dbContext.People.ToImmutableList();

        // Act
        var actual = _sut.Serialize(people);

        // Assert
        Assert.Equal("Id,Name\r\n1,Ford Person\r\n2,Chevy Person\r\n"u8.ToArray(), actual);
    }

    [Fact]
    public void Serialize_WhenSerializingEmptyCollection_ReturnsHeadersOnly()
    {
        // Arrange
        var emptyCollection = ImmutableList<Car>.Empty;

        // Act
        var actual = _sut.Serialize(emptyCollection);

        // Assert
        Assert.Equal("Id,Make,OwnerId\r\n"u8.ToArray(), actual);
    }

    [Fact]
    public void Serialize_WhenEntityDoesNotHaveMapper_ThrowsInvalidOperationException()
    {
        // Arrange
        ImmutableList<Motorcycle> nullCollection = new List<Motorcycle>().ToImmutableList();

        // Act
        var actual = Assert.Throws<ConfigurationException>(() => _sut.Serialize(nullCollection));

        // Assert
        Assert.Equal("Failed to get class map for Motorcycle. Did you add an entity and register the class map in the service provider?", actual.Message);
    }

    [Fact]
    public void Deserialize_WhenDeserializingCars_ReturnsExpectedResult()
    {
        // Arrange
        var input = "Id,Make,OwnerId\r\n1,Ford,1\r\n2,Chevy,3\r\n"u8.ToArray();

        // Act
        var actual = _sut.Deserialize<Car>(input);

        // Assert
        var actualList = actual.ToList();
        Assert.Equal(2, actualList.Count);
        Assert.Equal(1, actualList[0].Id);
        Assert.Equal("Ford", actualList[0].Make);
        Assert.Equal(1, actualList[0].OwnerId);
        Assert.Equal(2, actualList[1].Id);
        Assert.Equal("Chevy", actualList[1].Make);
        Assert.Equal(3, actualList[1].OwnerId);
    }

    [Fact]
    public void Deserialize_WhenEntityDoesNotHaveMapper_ThrowsInvalidOperationException()
    {
        // Arrange
        var input = "Id\r\n1\r\n2\r\n"u8.ToArray();

        // Act
        var actual = Assert.Throws<ConfigurationException>(() => _sut.Deserialize<Motorcycle>(input));

        // Assert
        Assert.Equal("Failed to get class map for Motorcycle. Did you add an entity and register the class map in the service provider?", actual.Message);
    }
}