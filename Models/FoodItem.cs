using SQLite;
using System.Text.Json.Serialization;

namespace FoodApp.Models;

[Table("FoodItems")]
public class FoodItem
{
    [PrimaryKey, AutoIncrement]
    [JsonIgnore]
    public int LocalId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [NotNull, Indexed]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [NotNull]
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

    public string? PhotoPath { get; set; }

    public string? LocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // time
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Add this property to FoodItem class
    public bool IsFavorite { get; set; } = false;

    [Ignore]
    public string CaloriesLabel => $"{Calories} kcal";

    [Ignore]
    public string NutritionSummary => $"P: {Protein}g | C: {Carbs}g | F: {Fat}g";

    [Ignore]
    public string SearchTags => $"{Name} {Category} {Description}".ToLower();
}