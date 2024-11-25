using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class FileCloudStorageMock(ILogger<FileCloudStorageMock> logger) : IFileCloudStorage
{
    public void DeleteFromCloud(string folder, string fileName, string extension)
    {
        logger.LogWarning("FileCloudStorageMock DeleteFromCloud : using mock implementation");
    }  

    public void SaveToCloud(string base64String, string folder, string fullFileName, string mediaType)
    {
        logger.LogWarning("FileCloudStorageMock SaveToCloud : using mock implementation");
    }

    public void SaveToCloud(Stream stream, string folder, string fullFileName, string mediaType)
    {
        logger.LogWarning("FileCloudStorageMock SaveToCloud : using mock implementation");
    }
    public byte[] DownloadFromCloud(string folder, string subFolder, string fileName)
    {
        var mockByte = new byte[16];
        return mockByte;
    }
}