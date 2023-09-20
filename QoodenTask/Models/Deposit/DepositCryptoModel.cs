using System.ComponentModel.DataAnnotations;

namespace QoodenTask.Models.Deposit;

public class DepositCryptoModel : BaseDepositModel
{
    [MaxLength(16)]
    [MinLength(16)]
    public string Address { get; set; }
}