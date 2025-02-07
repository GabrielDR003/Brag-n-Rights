using GymBro_App.Models;
using System.Text.Json;

namespace GymBro_App.Services;

//TODO: Conform these classes to the api's actual structure
class ExtApiFood
{
    public string food_id { get; set; } = "";
    public string food_name { get; set; } = "";
    public string food_description { get; set; } = "";
    public string brand_name { get; set; } = "";
}

class GetFoodResponse
{
    public ExtApiFood food { get; set; } = new ExtApiFood();
}

class FoodSearch
{
    public List<ExtApiFood> foods { get; set; } = new List<ExtApiFood>();
}

public class FoodService : IFoodService
{
    readonly HttpClient _httpClient;
    readonly ILogger<FoodService> _logger;

    public FoodService(HttpClient httpClient, ILogger<FoodService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ApiFood>> GetFoodsAsync()
    {
        var response = await _httpClient.GetAsync("api/foods");
        if (response.IsSuccessStatusCode)
        {
            var foods = await JsonSerializer.DeserializeAsync<List<ExtApiFood>>(await response.Content.ReadAsStreamAsync());
            foods = foods ?? new List<ExtApiFood>();
            return foods.Select(f => new ApiFood
            {
            }).ToList();
        }
        return null;
    }

    public async Task<ApiFood> GetFoodAsync(string id)
    {
        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string?, string?>("method", "food.get"),
                        new KeyValuePair<string?, string?>("food_id", id.ToString()),
                        new KeyValuePair<string?, string?>("format", "json")
                    }));
        var rawResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Raw Response: {rawResponse}");
        if (response.IsSuccessStatusCode)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var foods = await JsonSerializer.DeserializeAsync<GetFoodResponse>(await response.Content.ReadAsStreamAsync(), options);
            foods = foods ?? new GetFoodResponse();
            var food = foods.food;

            return new ApiFood
            {
                FoodName = food.food_name,
                FoodDescription = food.food_description,
                FoodId = food.food_id,
                BrandName = food.brand_name
            };
        }
        return null;
    }
}
