using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace QoodenTask.Models.Deposit;

public class DepositFiatModel : BaseDepositModel
{
    [JsonProperty("card_number")]
    [MaxLength(16)]
    [MinLength(16)]
    public string CardNumber { get; set; }
    
    [JsonProperty("cardholder_name")]
    [MaxLength(16)]
    [MinLength(2)]
    public string CardHolder { get; set; }
}