namespace Core.Utils.DB;

public interface IFileCloudStorage
{
   // public void SaveJpegToCloud(string base64String, string folder, string subFolder, string fileName);
    public void SaveToCloud(string base64String, string folder, string fullFileName, string mediaType);
    public void SaveToCloud(Stream stream, string folder, string fullFileName, string mediaType);
    public void DeleteFromCloud(string folder, string fileName, string extension);
    byte[] DownloadFromCloud(string folder, string subFolder, string fileName);
}