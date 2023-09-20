using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QoodenTask.Models.Deposit;
[JsonDerivedType(typeof(DepositCryptoModel), typeDiscriminator: nameof(DepositCryptoModel))]
[JsonDerivedType(typeof(DepositFiatModel), typeDiscriminator: nameof(DepositFiatModel))]
public abstract class BaseDepositModel
{
    [Range((double)0.1m,(double)100m)]
    public decimal Amount { get; set; }
}