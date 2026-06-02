namespace FoodApp.Services;

public static class PermissionService
{
    public static async Task<bool> RequestCameraPermissionAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> RequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }
}