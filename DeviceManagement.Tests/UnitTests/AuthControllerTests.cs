using DeviceManagement.Api.AppSettings;
using DeviceManagement.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace DeviceManagement.Tests.UnitTests;

public class AuthControllerTests
{
    private readonly Mock<IOptions<JwtSettings>> _jwtOptionsMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _jwtSettings = new JwtSettings
        {
            Issuer = "https://test-auth.example.com",
            Audience = "https://test-api.example.com",
            Secret = "this-is-a-very-long-and-secure-secret-1234567890abcdef", // ≥256 bits
            ExpirationInMinutes = 15
        };

        _jwtOptionsMock = new Mock<IOptions<JwtSettings>>();
        _jwtOptionsMock.Setup(o => o.Value).Returns(_jwtSettings);

        _loggerMock = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(_jwtOptionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Login_WhenUsernameOrPasswordIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var login = new AuthController.UserLogin("", "something");

        // Act
        var result = _controller.Login(login) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);

        var error = result.Value as dynamic;
        ((string)error!.Error).Should().Be("Username and password are required");
    }

    [Fact]
    public void Login_WhenCredentialsAreInvalid_ShouldReturnUnauthorized_AndLogWarning()
    {
        // Arrange
        var login = new AuthController.UserLogin("wrong", "wrong");

        // Act
        var result = _controller.Login(login);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();

        // Optional: verify logging (a bit fragile but useful)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed login attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once());
    }

    [Fact]
    public void Login_WhenJwtSettingsAreInvalid_ShouldReturn500()
    {
        // Arrange - override valid settings with invalid ones
        var badSettings = new JwtSettings { Issuer = "", Audience = "", Secret = "short", ExpirationInMinutes = 0 };
        _jwtOptionsMock.Setup(o => o.Value).Returns(badSettings);

        var controller = new AuthController(_jwtOptionsMock.Object, _loggerMock.Object);
        var login = new AuthController.UserLogin("test", "password01!");

        // Act
        var result = controller.Login(login) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void Login_WhenSecretIsTooShort_ShouldReturn500()
    {
        // Arrange
        var weakSettings = new JwtSettings
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Secret = "short123",  // < 32 bytes
            ExpirationInMinutes = _jwtSettings.ExpirationInMinutes
        };
        _jwtOptionsMock.Setup(o => o.Value).Returns(weakSettings);

        var controller = new AuthController(_jwtOptionsMock.Object, _loggerMock.Object);
        var login = new AuthController.UserLogin("test", "password01!");

        // Act
        var result = controller.Login(login) as ObjectResult;

        // Assert
        result!.StatusCode.Should().Be(500);
    }

    [Fact]
    public void Login_WhenCredentialsAreValid_ShouldReturnOkWithJwtToken()
    {
        // Arrange
        var login = new AuthController.UserLogin("test", "password01!");

        // Act
        var result = _controller.Login(login) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        dynamic response = result.Value!;
        string token = response.Token;
        int expiresIn = response.ExpiresInMinutes;
        string tokenType = response.TokenType;

        token.Should().NotBeNullOrEmpty();
        expiresIn.Should().Be(_jwtSettings.ExpirationInMinutes);
        tokenType.Should().Be("Bearer");

        // Basic token validation
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();

        var jwt = handler.ReadJwtToken(token);
        jwt.Issuer.Should().Be(_jwtSettings.Issuer);
        jwt.Audiences.Should().Contain(_jwtSettings.Audience);
        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes!.Value), precision: TimeSpan.FromSeconds(10));
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "test");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void Login_WhenValid_ShouldIncludeJtiClaim()
    {
        // Arrange & Act (reuse successful case)
        var login = new AuthController.UserLogin("test", "password01!");
        var result = _controller.Login(login) as OkObjectResult;
        dynamic resp = result!.Value!;
        string token = resp.Token;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        Guid.Parse(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value)
            .Should().NotBeEmpty();
    }
}