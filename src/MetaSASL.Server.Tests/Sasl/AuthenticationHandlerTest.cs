using Xunit;
using Moq;
using Sasl.Abstractions;
using Sasl.Configuration;

namespace Sasl.Tests;

public class AuthenticationHandlerTest
{
    private readonly Mock<ISocket> _socketMock = new();
    private readonly Mock<IAuthenticationService> _authMock = new();
    private readonly IRequestHandler _handler;

    public AuthenticationHandlerTest()
    {
        var config = new ServerConfiguration();
        config.Realms["example.org"] = _authMock.Object;
        _handler = new AuthenticationHandler(config);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Handle_InvalidLogin_NO(string login)
    {
        _socketMock.SetupSequence(m => m.ReadString())
            .Returns(login)
            .Returns("x")
            .Returns("ldap")
            .Returns("example.org");

        _handler.HandleRequest(_socketMock.Object);

        _socketMock.Verify(
            x => x.SendString(It.IsRegex(@"^NO Value cannot be null. \(Parameter 'login'\)$")),
            Times.Once());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Handle_InvalidPassword_NO(string password)
    {
        _socketMock.SetupSequence(m => m.ReadString())
            .Returns("fry")
            .Returns(password)
            .Returns("ldap")
            .Returns("example.org");

        _handler.HandleRequest(_socketMock.Object);

        _socketMock.Verify(
            x => x.SendString(It.IsRegex(@"^NO Value cannot be null. \(Parameter 'password'\)$")),
            Times.Once());
    }

    [Theory]
    [InlineData("acap")]
    [InlineData("host")]
    public void Handle_InvalidService_NO(string service)
    {
        _socketMock.SetupSequence(m => m.ReadString())
            .Returns("fry")
            .Returns("fry")
            .Returns(service)
            .Returns("example.org");

        _handler.HandleRequest(_socketMock.Object);

        _socketMock.Verify(
            x => x.SendString(It.IsRegex(@"^NO Service \w+ is unsupported$")),
            Times.Once());
    }

    [Theory]
    [InlineData("fry", "fry_")]
    [InlineData("fry_", "fry")]
    public void Handle_ValidRequestWithBadCredentials_NO(string login, string password)
    {
        _socketMock.SetupSequence(m => m.ReadString())
            .Returns(login)
            .Returns(password)
            .Returns("ldap")
            .Returns("example.org");

        _handler.HandleRequest(_socketMock.Object);

        _socketMock.Verify(x => x.SendString(It.IsRegex("^NO$")));
    }

    [Fact]
    public void Handle_ValidRequestWithRightCredentials_OK()
    {
        _authMock.Setup(x => x.Authenticate("fry", "fry", "ldap", "example.org")).Returns(true);
        _socketMock.SetupSequence(m => m.ReadString())
            .Returns("fry")
            .Returns("fry")
            .Returns("ldap")
            .Returns("example.org");

        _handler.HandleRequest(_socketMock.Object);

        _socketMock.Verify(x => x.SendString(It.IsRegex("^OK$")));
    }
}
