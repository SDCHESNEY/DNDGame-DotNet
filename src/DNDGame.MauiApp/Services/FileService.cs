using DNDGame.MauiApp.Interfaces;

namespace DNDGame.MauiApp.Services;

public class FileService : IFileService
{
    public async Task<Stream?> OpenReadAsync(string fileName)
    {
        var fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        
        if (!File.Exists(fullPath))
        {
            return null;
        }
        
        return await Task.FromResult(File.OpenRead(fullPath));
    }

    public async Task<string> SaveFileAsync(string fileName, byte[] data)
    {
        var fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllBytesAsync(fullPath, data);
        return fullPath;
    }

    public async Task<byte[]?> ReadFileAsync(string fileName)
    {
        var fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        
        if (!File.Exists(fullPath))
        {
            return null;
        }
        
        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> DeleteFileAsync(string fileName)
    {
        var fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public async Task<string> PickFileAsync(params string[] allowedTypes)
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, allowedTypes },
                { DevicePlatform.Android, allowedTypes },
                { DevicePlatform.WinUI, allowedTypes },
                { DevicePlatform.macOS, allowedTypes }
            });

            var options = new PickOptions
            {
                FileTypes = customFileType,
                PickerTitle = "Select a file"
            };

            var result = await FilePicker.Default.PickAsync(options);
            return result?.FullPath ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public async Task<string> SaveToDownloadsAsync(string fileName, byte[] data)
    {
        try
        {
            // For mobile platforms, save to a shareable location
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, data);
            
            // Share the file so user can save it
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save File",
                File = new ShareFile(filePath)
            });
            
            return filePath;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public Task<bool> FileExistsAsync(string fileName)
    {
        var fullPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        return Task.FromResult(File.Exists(fullPath));
    }
}
