using Core.Utils.FTP.Models;
using FluentFTP;
using FluentFTP.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace Core.Utils.FTP.Services
{
   
    [ExcludeFromCodeCoverage]
    public class FTPService(ILogger<FTPService> logger) : IFTPService
    {
        private FtpClient? client;
        public void Create(bool isFTPS, string Host, int Port, string userId, string password, Encoding encoding, FtpEncryptionMode encryptionMode = FtpEncryptionMode.Implicit, FtpDataConnectionType connectionType = FtpDataConnectionType.PASV)
        {
            try
            {
                client = new FtpClient();
                client.Host = Host;
                client.Port = Port;
                client!.Encoding = encoding;
                client.Credentials = new NetworkCredential(userId, password);
                client.Config.EncryptionMode = encryptionMode;
                client.Config.DataConnectionEncryption = isFTPS;
                client.Config.DataConnectionType = connectionType;
                if (isFTPS)
                {
                    client.ValidateCertificate += new FtpSslValidation((control, e) =>
                    {
                        e.Accept = true;
                    });
                }
                ClientConnect();
                logger.LogInformation("FTPService Create: {info}", $"FTP Client Created");
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService GetFileDetail: {info}", $"{ex.Message}");
                throw;
            }

        }
        public FTPFileDetail? GetFileDetail(string fullname) {

            CheckClientExitsAndConnectIfNot();
            try
            {
                if (!client!.FileExists(fullname)){
                    return null;
                }
                var item = client!.GetObjectInfo(fullname);

                if (item.Type == FtpObjectType.File)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var ftpStream = client.OpenRead(item.FullName))
                        {
                            ftpStream.CopyTo(memoryStream);
                            memoryStream.Position = 0;
                        }

                        byte[] fileInByteArray = memoryStream.ToArray();
                        memoryStream.Dispose();

                        logger.LogInformation("FTPService GetFileDetail: {info}", $"Get file detail from FTP Server");
                        return new FTPFileDetail
                        {
                            Full_Name = item.FullName,
                            Name = item.Name,
                            Created_Date = item.Created,
                            Last_Modified_Date = item.Modified,
                            File_Size = item.Size,
                            File_Bytes = fileInByteArray,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService GetFileDetail: {info}", $"{ex.Message}");
                throw;
            }
            return null;
        }
        public string GetWorkingDirectory() {
            CheckClientExitsAndConnectIfNot();
            return client!.GetWorkingDirectory().TrimEnd('/');
        }
        public void Dispose() { 
            if(client != null)
                client.Dispose();
        }
        public List<FTPDirectoryContent> GetDirectoryContent(string pathfolder)
        {

            var fileList = new List<FTPDirectoryContent>();
            try
            {
                CheckClientExitsAndConnectIfNot();
                var items = client!.GetListing(pathfolder);
                fileList = items.Select(item =>
                    new FTPDirectoryContent() { Full_Name = item.FullName, Name = item.Name, File_Type = item.Type, File_Size = item.Size, Created_Date = item.Created, Last_Modified_Date = item.Modified, Is_Folder = item.Type != FtpObjectType.File }
                ).ToList();
                logger.LogInformation("FTPService GetDirectoryContent: {info}", $"Get dicrectory content from FTP Server");
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService GetDirectoryContent: {info}", $"{ex.Message}");
                throw;
            }
            return fileList;
        }
        public FtpListItem GetObjectInfo(string fullpath)
        {

            try
            {
                CheckClientExitsAndConnectIfNot();
                var items = client!.GetObjectInfo(fullpath);

                return items;
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService GetObjectInfo: {info}", $"{ex.Message}");
                throw;
            }
        }
        public bool UploadFile(string sourceFile, string pathfolder, string filename, FtpRemoteExists whenExists = FtpRemoteExists.Overwrite, bool createDir = true)
        {

            try
            {
                CheckClientExitsAndConnectIfNot();
                if (!File.Exists(sourceFile)) {
                    logger.LogInformation("FTPService UploadFile: {info}", $"Source File not found");
                    return false;
                }

                pathfolder = pathfolder.TrimEnd('/') + "/" + filename;
                if (client!.UploadFile(sourceFile, pathfolder , whenExists, createDir).IsSuccess())
                {
                    logger.LogInformation("FTPService UploadFile: {info}", $"Uploaded file to FTP Server");
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService UploadFile: {info}", $"{ex.Message}");
                throw;
            }
            
        }
        public bool UploadFile(byte[] sourceFile, string pathfolder, string filename, FtpRemoteExists whenExists = FtpRemoteExists.Overwrite, bool createDir = true)
        {
            try
            {
                CheckClientExitsAndConnectIfNot();
                pathfolder = pathfolder.TrimEnd('/');
                logger.LogInformation($"Created directory {pathfolder}");

                if (client!.UploadBytes(sourceFile, $"{pathfolder}/{filename}", whenExists, createDir).IsSuccess())
                {
                    logger.LogInformation("FTPService UploadFile: {info}", $"Uploaded file to FTP Server");
                    return true;
                }
                else
                {
                    logger.LogInformation("FTPService UploadFile: {info}", $"Failed Upload file to FTP Server");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService UploadFile: {info}", $"{ex.Message}");
                throw;
            }

        }
        public bool ItemExists(string fullpath) {
            try
            {
                CheckClientExitsAndConnectIfNot();
                var item = GetObjectInfo(fullpath);
                return item != null;
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService ItemExists: {info}", $"{ex.Message}");
                throw;
            }
        }
        public void DeleteFile(string fullpath) {
            try
            {
                CheckClientExitsAndConnectIfNot();
                if (ItemExists(fullpath)) {
                    client!.DeleteFile(fullpath);
                }
            } catch (Exception ex) 
            {
                logger.LogInformation("FTPService DeleteFile: {info}", $"{ex.Message}");
                throw;
            
            }

        }
        public bool CreateDirectory(string fullpath, bool force = false) {
            try
            {
                CheckClientExitsAndConnectIfNot();
                return client!.CreateDirectory(fullpath, force);
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService DeleteDirectory: {info}", $"{ex.Message}");
                throw;
            }
        }
        public void DeleteDirectory(string fullpath, FtpListOption ftpListOption = FtpListOption.Auto)
        {
            try
            {
                CheckClientExitsAndConnectIfNot();
                if (ItemExists(fullpath))
                {
                    client!.DeleteDirectory(fullpath, ftpListOption);
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation("FTPService DeleteDirectory: {info}", $"{ex.Message}");
                throw;
            }

        }
        public FtpClient GetFTPClient() {
            CheckClientExitsAndConnectIfNot();
            return client!;
        }
        public void ClientConnect()
        {
            if (!client!.IsConnected)
            {
                logger.LogDebug("FTPService ClientConnect: {info}", $"Connecting to FTP Server" );
                client!.Connect();
            }
        }
        public void ClientDisconnect()
        {
            if (client!.IsConnected) {
                logger.LogDebug("FTPService ClientDisconnect: {info}", $"Disconnecting from FTP Server");
                client!.Disconnect();
                logger.LogDebug("FTPService ClientDisconnect: {info}", $"Disconnected from FTP Server");
            }
        }
        private void CheckClientExitsAndConnectIfNot() {
            if (client == null)
            {
                logger.LogDebug("FTPService CheckClientExitsAndConnectIfNot: {info}", $"FTP Client not set yet");
                throw new ArgumentException("FTP Client not set yet");
            }
            else {
                ClientConnect();
            }
        }
    }
}
