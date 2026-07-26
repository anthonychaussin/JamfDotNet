using JamfDotNet.Core;

namespace JamfDotNet.Classic.Tests;

public sealed class JamfClientOptionsClassicTests
{
    [Fact]
    public void ClassicApiBaseUrl_Appends_JSSResource()
    {
        var options = new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            ClientId = "id",
            ClientSecret = "secret",
        };

        options.Validate();
        Assert.Equal("https://example.jamfcloud.com/JSSResource/", options.ClassicApiBaseUrl.AbsoluteUri);
        Assert.Equal("https://example.jamfcloud.com/api/", options.ApiBaseUrl.AbsoluteUri);
    }
}
