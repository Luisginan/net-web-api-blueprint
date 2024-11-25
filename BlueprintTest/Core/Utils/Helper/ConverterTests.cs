using Newtonsoft.Json;
using System.Net.Http.Headers;
using Core.Utils.Helper;

namespace BlueprintTest.Core.Utils;
public class ConverterTests
{
    [Fact]
    public void HeadersToJson_ShouldReturnJson_WhenHttpHeadersIsNotNull()
    {
        // Arrange
        HttpHeaders headers = new HttpRequestMessage().Headers;
        headers.Add("Header1", "Value1");
        headers.Add("Header2", new List<string> { "Value2", "Value3" });

        // Act
        string result = Converter.HeadersToJson(headers);

        // Assert
        var expectedDict = new Dictionary<string, IEnumerable<string>>()
        {
            { "Header1", new List<string> { "Value1" } },
            { "Header2", new List<string> { "Value2", "Value3" } }
        };
        string expectedJson = JsonConvert.SerializeObject(expectedDict, Formatting.Indented);
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public void HeadersToJson_ShouldReturnEmptyString_WhenHttpHeadersIsNull()
    {
        // Arrange
        HttpHeaders headers = null;

        // Act
        string result = Converter.HeadersToJson(headers);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void HeadersToJson_ShouldReturnJson_WhenHttpRequestHeadersIsNotNull()
    {
        // Arrange
        HttpRequestHeaders headers = new HttpRequestMessage().Headers;
        headers.Add("Header1", "Value1");
        headers.Add("Header2", new List<string> { "Value2", "Value3" });

        // Act
        string result = Converter.HeadersToJson(headers);

        // Assert
        var expectedDict = new Dictionary<string, IEnumerable<string>>()
        {
            { "Header1", new List<string> { "Value1" } },
            { "Header2", new List<string> { "Value2", "Value3" } }
        };
        string expectedJson = JsonConvert.SerializeObject(expectedDict, Formatting.Indented);
        Assert.Equal(expectedJson, result);
    }

    [Fact]
    public void HeadersToJson_ShouldReturnEmptyString_WhenHttpRequestHeadersIsNull()
    {
        // Arrange
        HttpRequestHeaders headers = null;

        // Act
        string result = Converter.HeadersToJson(headers);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
