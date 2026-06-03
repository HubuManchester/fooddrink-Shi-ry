using SQLite;
using FoodApp.Models;

namespace FoodApp.Services;

public class FoodDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private static bool _isInitialized = false;
    private static readonly object _lock = new object();
    private readonly string _databasePath;

    public FoodDatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "foodapp.db3");
    }

    private async Task InitAsync()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;
            _database = new SQLiteAsyncConnection(_databasePath);
        }

        await _database.CreateTableAsync<FoodItem>();
        _isInitialized = true;
    }

    // add
    public async Task<int> AddAsync(FoodItem item)
    {
        await InitAsync();
        if (_database is null) return 0;

        System.Diagnostics.Debug.WriteLine($"=== AddAsync: Adding {item.Name}, Id={item.Id} ===");

        var result = await _database.InsertAsync(item);

        System.Diagnostics.Debug.WriteLine($"=== AddAsync: Result={result}, LocalId={item.LocalId} ===");

        return result;
    }

    // get all
    public async Task<List<FoodItem>> GetAllAsync()
    {
        await InitAsync();
        if (_database is null) return new List<FoodItem>();
        return await _database.Table<FoodItem>().ToListAsync();
    }

    public async Task<FoodItem?> GetByIdAsync(string id)
    {
        await InitAsync();
        if (_database is null) return null;
        return await _database.Table<FoodItem>().FirstOrDefaultAsync(x => x.Id == id);
        // Use FirstOrDefaultAsync directly on the table
        var result = await _database.Table<FoodItem>().FirstOrDefaultAsync(x => x.Id == id);

        if (result != null)
        {
            System.Diagnostics.Debug.WriteLine($"=== GetByIdAsync: Found {result.Name}, LocalId={result.LocalId} ===");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"=== GetByIdAsync: No item found with Id={id} ===");
        }

        return result;
    }

    public async Task<FoodItem?> GetByLocalIdAsync(int localId)
    {
        await InitAsync();
        if (_database is null) return null;
        return await _database.Table<FoodItem>().FirstOrDefaultAsync(x => x.LocalId == localId);
    }

    // search
    public async Task<List<FoodItem>> SearchAsync(string? query)
    {
        await InitAsync();
        if (_database is null) return new List<FoodItem>();

        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        var normalized = query.Trim().ToLower();
        return await _database
            .Table<FoodItem>()
            .Where(x => x.Name.ToLower().Contains(normalized) ||
                        x.Category.ToLower().Contains(normalized) ||
                        x.Description.ToLower().Contains(normalized))
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    // update
    public async Task<int> UpdateAsync(FoodItem item)
    {
        await InitAsync();
        if (_database is null) return 0;

        System.Diagnostics.Debug.WriteLine($"=== UpdateAsync: LocalId={item.LocalId}, Name={item.Name}, IsFavorite={item.IsFavorite} ===");

        if (item.LocalId == 0)
        {
            System.Diagnostics.Debug.WriteLine("=== ERROR: LocalId is 0, cannot update! ===");
            return 0;
        }

        var result = await _database.UpdateAsync(item);
        System.Diagnostics.Debug.WriteLine($"=== UpdateAsync result: {result} rows affected ===");

        return result;
    }

    // delete by id
    public async Task<int> DeleteAsync(string id)
    {
        await InitAsync();
        if (_database is null) return 0;
        var item = await GetByIdAsync(id);
        if (item is null) return 0;
        return await _database.DeleteAsync(item);
    }

    // delete by item
    public async Task<int> DeleteAsync(FoodItem item)
    {
        await InitAsync();
        if (_database is null) return 0;
        return await _database.DeleteAsync(item);
    }

    // Get favorite items only
    public async Task<List<FoodItem>> GetFavoritesAsync()
    {
        await InitAsync();
        if (_database is null) return new List<FoodItem>();
        return await _database.Table<FoodItem>().Where(x => x.IsFavorite == true).ToListAsync();
    }

    // Add this method to FoodDatabaseService.cs
    public async Task<int> SetFavoriteAsync(string id, bool isFavorite)
    {
        await InitAsync();
        if (_database is null) return 0;

        // First check if the item exists
        var existingItem = await _database.Table<FoodItem>().FirstOrDefaultAsync(x => x.Id == id);

        if (existingItem == null)
        {
            System.Diagnostics.Debug.WriteLine($"=== SetFavoriteAsync: Item with Id={id} NOT FOUND in database! ===");
            return 0;
        }

        System.Diagnostics.Debug.WriteLine($"=== SetFavoriteAsync: Found item: {existingItem.Name}, Current IsFavorite={existingItem.IsFavorite}, LocalId={existingItem.LocalId} ===");

        // Use LocalId for update instead of Id
        var result = await _database.ExecuteAsync(
            "UPDATE FoodItems SET IsFavorite = ? WHERE LocalId = ?",
            isFavorite ? 1 : 0,
            existingItem.LocalId);

        System.Diagnostics.Debug.WriteLine($"=== SetFavoriteAsync: Rows affected={result} ===");

        return result;
    }
}