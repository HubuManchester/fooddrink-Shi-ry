using System.Text.Json.Serialization;

namespace FoodApp.Models;

public class FoodItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("calories")]
    public int Calories { get; set; }

    [JsonPropertyName("protein")]
    public int Protein { get; set; }

    [JsonPropertyName("carbs")]
    public int Carbs { get; set; }

    [JsonPropertyName("fat")]
    public int Fat { get; set; }

    [JsonPropertyName("allergyInfo")]
    public string AllergyInfo { get; set; } = "No allergy information provided.";

    public string CaloriesLabel => $"{Calories} kcal";
    public string NutritionSummary => $"P: {Protein}g | C: {Carbs}g | F: {Fat}g";
    public string SearchTags => $"{Name} {Category} {Description}".ToLower();
}