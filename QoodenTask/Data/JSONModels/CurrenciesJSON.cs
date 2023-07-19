using Newtonsoft.Json;
using QoodenTask.Models;

namespace QoodenTask.Data.JSONModels;

public class CurrenciesJSON
{
    [JsonProperty("currencies")]
    public Currency[] Currencies { get; set; }
}