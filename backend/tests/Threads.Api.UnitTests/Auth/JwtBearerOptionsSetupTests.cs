using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Threads.Api.Features.Auth.Services.JwtProvider;

namespace Threads.Api.UnitTests.Auth;

public class JwtBearerOptionsSetupTests
{
    [Fact]
    public void Configure_ShouldConfigureBearerTokenValidationParameters()
    {
        // Arrange
        var jwtOptions = OptionsTestFactory.CreateJwtOptions();
        var setup = new JwtBearerOptionsSetup(Options.Create(jwtOptions));
        var options = new JwtBearerOptions();

        // Act
        setup.Configure(JwtBearerDefaults.AuthenticationScheme, options);

        // Assert
        var parameters = options.TokenValidationParameters;
        parameters.ValidateIssuer.Should().BeTrue();
        parameters.ValidateAudience.Should().BeTrue();
        parameters.ValidateLifetime.Should().BeTrue();
        parameters.ValidateIssuerSigningKey.Should().BeTrue();
        parameters.ValidIssuer.Should().Be(jwtOptions.Issuer);
        parameters.ValidAudience.Should().Be(jwtOptions.Audience);
        parameters.ClockSkew.Should().Be(TimeSpan.Zero);

        var signingKey = parameters
            .IssuerSigningKey.Should()
            .BeOfType<SymmetricSecurityKey>()
            .Which;

        signingKey.Key.Should().Equal(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
    }

    [Fact]
    public void Configure_ShouldNotConfigureNonBearerSchemes()
    {
        // Arrange
        var jwtOptions = OptionsTestFactory.CreateJwtOptions();
        var setup = new JwtBearerOptionsSetup(Options.Create(jwtOptions));
        var options = new JwtBearerOptions();

        // Act
        setup.Configure("OtherScheme", options);

        // Assert
        var parameters = options.TokenValidationParameters;
        parameters.ValidIssuer.Should().BeNull();
        parameters.ValidAudience.Should().BeNull();
        parameters.IssuerSigningKey.Should().BeNull();
    }
}
