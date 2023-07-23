using Newtonsoft.Json;

namespace QoodenTask.Services;

public class JsonService
{
    /*public async Task<List<User>> GetUsersFromJson(string fileName = "Users")
    {
        try
        {
            var usersJson = File.ReadAllText($@"..\QoodenTask\{fileName}.json");
            return JsonConvert.DeserializeObject<UsersJSON>(usersJson)?.Users;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("File not found");
        }

        return null;
    }
    
    public async Task<List<Currency>> GetCurrenciesFromJson<T>(string fileName = "Curencies")
    {
        try
        {
            var currenciesJson = File.ReadAllText($@"..\QoodenTask\{fileName}.json");
            return JsonConvert.DeserializeObject<CurrenciesJSON>(currenciesJson)?.Currencies;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("File not found");
        }

        return null;
    }*/
    
    public async Task<List<T>> GetDataFromJson<T>(string fileName)
    {
        try
        {
            var dataJson = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<List<T>>(dataJson);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("File not found");
        }

        return null;
    }
}