namespace DNDGame.MauiApp.Interfaces;

public interface IFileService
{
    Task<Stream?> OpenReadAsync(string fileName);
    Task<string> SaveFileAsync(string fileName, byte[] data);
    Task<byte[]?> ReadFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
    Task<string> PickFileAsync(params string[] allowedTypes);
    Task<string> SaveToDownloadsAsync(string fileName, byte[] data);
    Task<bool> FileExistsAsync(string fileName);
}