using Newtonsoft.Json;

namespace QoodenTask.Models;

public class UserBalance
{
    public decimal Balance { get; set; }
    
    [JsonProperty("usd_amount")]
    public decimal UsdAmount { get; set; }
}