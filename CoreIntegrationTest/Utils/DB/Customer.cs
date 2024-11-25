using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Core.Utils.DB;
using Newtonsoft.Json;

namespace CoreIntegrationTest.Utils.DB;

[ExcludeFromCodeCoverage]
[Table("customer","id")]
public class Customer
{
    [Field("id")]
    [JsonProperty("id")]
    [DataMember]
    public int Id { get; set; }
    [Field("name")]
    [JsonProperty("name")]
    [DataMember]
   
    public string Name { get; set; } = "";
    [Field("address")]
    [JsonProperty("address")]
    [DataMember]
    public string Address { get; set; } = "";
    [Field("email")]
    [JsonProperty("email")]
    [DataMember]

    public string Email2 { get; set; }  = "";
    [Field("phone")]
    [JsonProperty("phone")]
    [DataMember]
    public string Phone { get; set; } = "";
        
    [Field("is_active")]
    [JsonProperty("is_active")]
    [DataMember]
    public bool IsActive { get; set; } = true;

}