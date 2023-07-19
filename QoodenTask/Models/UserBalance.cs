using Newtonsoft.Json;

namespace QoodenTask.Models;

public class UserBalance
{
    public decimal Balance { get; set; }
    [JsonProperty("")]
    public decimal UsdAmount { get; set; }
}