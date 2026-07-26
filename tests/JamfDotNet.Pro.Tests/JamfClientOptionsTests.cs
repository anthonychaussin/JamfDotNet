using JamfDotNet.Core;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfClientOptionsTests
{
    [Fact]
    public void Validate_Throws_When_BaseUrl_Missing()
    {
        var options = new JamfClientOptions
        {
            ClientId = "id",
            ClientSecret = "secret",
        };

        var ex = Assert.Throws<InvalidOperationException>(options.Validate);
        Assert.Contains(nameof(JamfClientOptions.BaseUrl), ex.Message);
    }

    [Fact]
    public void Validate_Throws_When_ClientCredentials_Missing_Secret()
    {
        var options = new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.ClientCredentials,
            ClientId = "id",
        };

        var ex = Assert.Throws<InvalidOperationException>(options.Validate);
        Assert.Contains(nameof(JamfClientOptions.ClientSecret), ex.Message);
    }

    [Fact]
    public void Validate_Succeeds_For_ClientCredentials()
    {
        var options = new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.ClientCredentials,
            ClientId = "id",
            ClientSecret = "secret",
        };

        options.Validate();
        Assert.Equal("https://example.jamfcloud.com/api/", options.ApiBaseUrl.AbsoluteUri);
    }

    [Fact]
    public void Validate_Succeeds_For_BasicToken()
    {
        var options = new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.BasicToken,
            Username = "admin",
            Password = "pass",
        };

        options.Validate();
    }
}
