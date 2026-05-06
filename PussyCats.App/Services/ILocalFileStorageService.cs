namespace PussyCats.App.Services;

public interface ILocalFileStorageService
{
    string SaveFile(Stream fileStream, string originalFileName);

    void DeleteFile(string relativePath);

    string GetFilePath(string relativePath);
}
