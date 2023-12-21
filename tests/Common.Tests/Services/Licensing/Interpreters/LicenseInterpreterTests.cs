using AutoFixture;
using Passwordless.Common.Services.Licensing.Interpreters;
using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Tests.Services.Licensing.Interpreters;

public class LicenseInterpreterTests
{
    private readonly Fixture _fixture = new();
    private readonly LicenseInterpreter _sut = new();

    [Fact]
    public void Generate_Returns_ExpectedObject_ForParameters()
    {
        // arrange
        var parameters = _fixture.Build<LicenseParameters>().With(x => x.ManifestVersion, 1).Create();
        
        // act
        var actual = _sut.Generate(parameters);
        
        // assert
        Assert.Equal(parameters.InstallationId, actual.InstallationId);
        Assert.Equal(parameters.ManifestVersion, actual.ManifestVersion);
        Assert.Equal(parameters.Plans.Count, actual.Plans.Count);
        Assert.Equal(parameters.Plans.First().Key, actual.Plans.First().Key);
        var actualFirstPlan = (Plan)actual.Plans.First().Value;
        Assert.Equal(parameters.Plans.First().Value.Seats, actualFirstPlan.Seats);
        Assert.Equal(parameters.Plans.First().Value.SupportsAuditLogging, actualFirstPlan.SupportsAuditLogging);
    }
    
    [Fact]
    public void Generate_Throws_ArgumentNullException_WhenParametersIsNull()
    {
        // arrange
        LicenseParameters? parameters = null;
        
        // act
        var exception = Assert.Throws<ArgumentNullException>(() => _sut.Generate(parameters));
        
        // assert
        Assert.Equal("Value cannot be null. (Parameter 'parameters')", exception.Message);
    }
}