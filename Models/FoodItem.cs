using SQLite;
using System.Text.Json.Serialization;

namespace FoodApp.Models;

[Table("FoodItems")]
public class FoodItem
{
    // 数据库主键：自增整型（仅本地使用）
    [PrimaryKey, AutoIncrement]
    [JsonIgnore]  // 忽略 JSON 序列化，因为 API 返回的是字符串 id
    public int LocalId { get; set; }

    // API 返回的字符串 ID（用于路由导航和 API 交互）
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

    // 照片路径（新增）
    public string? PhotoPath { get; set; }

    // 位置信息（新增）
    public string? LocationName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // 时间戳
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