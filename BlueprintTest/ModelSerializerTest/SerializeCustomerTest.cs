using Blueprint.CustomerModule.Models;
using Newtonsoft.Json;

namespace BlueprintTest.ModelSerializerTest;

public class SerializeCustomerTest
{
    [Fact]
    public void TestSerializeCustomer()
    {
        // Arrange
        var customer = new CustomerPayload
        {
            Id = "1",
            Message = "Hello"
        };
        var expected = "{\"id\":\"1\",\"message\":\"Hello\"}";

        // Act
        var actual = JsonConvert.SerializeObject(customer);

        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void TestDeserializeCustomer()
    {
        // Arrange
        var json = "{\"id\":\"1\",\"message\":\"Hello\"}";
        var expected = new CustomerPayload
        {
            Id = "1",
            Message = "Hello"
        };

        // Act
        var actual = JsonConvert.DeserializeObject<CustomerPayload>(json);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Message, actual.Message);
    }
}