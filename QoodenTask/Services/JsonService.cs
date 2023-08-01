using Newtonsoft.Json;

namespace QoodenTask.Services;

public static class JsonService
{
    public static async Task<List<T>?> GetDataFromJson<T>(string fileName)
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