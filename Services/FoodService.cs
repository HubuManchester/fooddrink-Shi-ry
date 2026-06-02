using System.Collections.ObjectModel;
using FoodApp.Models;

namespace FoodApp.Services;

public class FoodService
{
    private readonly List<FoodItem> _localData;

    public ObservableCollection<FoodItem> Items { get; private set; }

    public FoodService()
    {
        _localData = new List<FoodItem>
        {
            new() { Name = "Greek Yogurt Bowl", Category = "Breakfast", Description = "Greek yogurt with honey, walnuts, and fresh berries.", Calories = 320, Protein = 24, Carbs = 35, Fat = 12, AllergyInfo = "Contains dairy, nuts." },
            new() { Name = "Avocado Toast", Category = "Breakfast", Description = "Whole grain toast with smashed avocado, chili flakes, and lemon juice.", Calories = 380, Protein = 12, Carbs = 42, Fat = 22, AllergyInfo = "Contains gluten." },
            new() { Name = "Grilled Chicken Salad", Category = "Lunch", Description = "Grilled chicken breast over mixed greens with cherry tomatoes and balsamic dressing.", Calories = 420, Protein = 38, Carbs = 18, Fat = 22, AllergyInfo = "No common allergens." },
            new() { Name = "Quinoa Buddha Bowl", Category = "Lunch", Description = "Quinoa with roasted vegetables, chickpeas, and tahini dressing.", Calories = 550, Protein = 18, Carbs = 68, Fat = 24, AllergyInfo = "Contains sesame." },
            new() { Name = "Salmon Rice Bowl", Category = "Dinner", Description = "Baked salmon with brown rice, steamed broccoli, and soy ginger sauce.", Calories = 620, Protein = 42, Carbs = 52, Fat = 28, AllergyInfo = "Contains fish, soy." },
            new() { Name = "Mushroom Risotto", Category = "Dinner", Description = "Creamy arborio rice with porcini mushrooms and parmesan cheese.", Calories = 580, Protein = 16, Carbs = 88, Fat = 18, AllergyInfo = "Contains dairy, gluten." },
            new() { Name = "Protein Smoothie", Category = "Drink", Description = "Banana, whey protein, almond milk, and peanut butter.", Calories = 290, Protein = 32, Carbs = 28, Fat = 10, AllergyInfo = "Contains dairy, nuts." },
            new() { Name = "Matcha Latte", Category = "Drink", Description = "Ceremonial matcha with steamed oat milk.", Calories = 140, Protein = 4, Carbs = 18, Fat = 6, AllergyInfo = "Dairy-free option available." },
            new() { Name = "Dark Chocolate Bar", Category = "Snack", Description = "70% dark chocolate with almonds and sea salt.", Calories = 210, Protein = 5, Carbs = 16, Fat = 14, AllergyInfo = "Contains nuts, soy." },
            new() { Name = "Apple Slices", Category = "Snack", Description = "Fresh apple slices with cinnamon sprinkle.", Calories = 95, Protein = 1, Carbs = 25, Fat = 0, AllergyInfo = "No common allergens." }
        };

        Items = new ObservableCollection<FoodItem>(_localData);
    }

    public async Task<IEnumerable<FoodItem>> SearchAsync(string? query)
    {
        await Task.Delay(100); // Simulate network delay

        if (string.IsNullOrWhiteSpace(query))
            return Items.OrderBy(x => x.Name);

        var normalized = query.Trim().ToLower();
        return Items.Where(x => x.SearchTags.Contains(normalized)).OrderBy(x => x.Name);
    }

    public async Task<FoodItem?> GetByIdAsync(string id)
    {
        await Task.Delay(50);
        return Items.FirstOrDefault(x => x.Id == id);
    }

    public async Task<FoodItem> AddAsync(FoodItem item)
    {
        await Task.Delay(200);
        item.Id = Guid.NewGuid().ToString();
        Items.Add(item);
        return item;
    }

    public async Task<bool> UpdateAsync(FoodItem item)
    {
        await Task.Delay(200);
        var existing = Items.FirstOrDefault(x => x.Id == item.Id);
        if (existing is null) return false;

        var index = Items.IndexOf(existing);
        Items[index] = item;
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        await Task.Delay(200);
        var existing = Items.FirstOrDefault(x => x.Id == id);
        if (existing is null) return false;

        Items.Remove(existing);
        return true;
    }
}