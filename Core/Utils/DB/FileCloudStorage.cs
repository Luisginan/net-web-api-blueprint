using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Core.Config;
using Core.Utils.Security;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace Core.Utils.DB;

[ExcludeFromCodeCoverage]
public class FileCloudStorage(ILogger<FileCloudStorage> logger, IOptions<FileStorageConfig> options , IVault vault)
    : IFileCloudStorage
{
    private readonly string _bucketName = vault.RevealSecret(options.Value).BucketName;
    private readonly ActivitySource _activitySource = new("file.storage");


    public void SaveToCloud(string base64String, string folder, string fullFileName, string mediaType)
    {
        var imageBytes = Convert.FromBase64String(base64String);
        Stream content = new MemoryStream(imageBytes);
        SaveToCloud(content, folder, fullFileName, mediaType);     
       
    }

    public void SaveToCloud(Stream stream, string folder, string fullFileName, string mediaType)
    {

        // use constants : MediaTypeNames._ for mediaType parameter    
        // ex jpg use :  MediaTypeNames.Image.Jpeg

        using var activity = _activitySource.StartActivity();
        AddFolder(folder);
        var client = StorageClient.Create();
        client.UploadObject(
            _bucketName,
            folder + "/" + fullFileName,
            mediaType,
            stream);

        logger.LogDebug("FileCloudStorage SaveToCloud: {folder}/{fullFileName}", folder, fullFileName);

        activity?.SetTag("folder", folder);
        activity?.SetTag("fileName", fullFileName);

    }

    public void DeleteFromCloud(string folder, string fileName, string extension)
    {
        extension = extension.ToLower();

        using var activity = _activitySource.StartActivity();
        AddFolder(folder);

        var client = StorageClient.Create();
        client.DeleteObject(
            _bucketName,
            folder + "/" + fileName + "." + extension);

        logger.LogDebug("FileCloudStorage DeleteFromCloud: {folder}/{fileName}.{extension}", folder, fileName, extension);

        activity?.SetTag("folder", folder);
        activity?.SetTag("fileName", fileName);

    }

        public byte[] DownloadFromCloud(string folder, string subFolder, string fileName)
    {
        using var activity = _activitySource.StartActivity();

        var client = StorageClient.Create();
        var objectName = folder + "/" + subFolder + "/" + fileName;

        using var memoryStream = new MemoryStream();
        client.DownloadObject(_bucketName, objectName, memoryStream);

        logger.LogDebug("FileCloudStorage DownloadFromCloud: {folder}/{subFolder}/{fileName}", folder, subFolder, fileName);

        activity?.SetTag("folder", folder);
        activity?.SetTag("subFolder", subFolder);
        activity?.SetTag("fileName", fileName);

        return memoryStream.ToArray();
    }


    private void AddFolder(string folderName)
    {
        var storageClient = StorageClient.Create();
        if (!folderName.EndsWith('/')) folderName += "/";
        var content = Encoding.UTF8.GetBytes("");
        storageClient.UploadObject(_bucketName, folderName, "application/x-directory", new MemoryStream(content));
    }
}