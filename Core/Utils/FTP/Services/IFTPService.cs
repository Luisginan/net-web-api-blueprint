
using FluentFTP;
using Core.Utils.FTP.Models;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.FTP.Services
{
    public interface IFTPService
    {
        void Create(bool isFTPS, string Host, int Port, string userId, string password, Encoding encoding, FtpEncryptionMode encryptionMode = FtpEncryptionMode.Implicit, FtpDataConnectionType connectionType = FtpDataConnectionType.PASV);
        string GetWorkingDirectory();
        List<FTPDirectoryContent> GetDirectoryContent(string pathfolder);
        FtpClient GetFTPClient();
        FTPFileDetail? GetFileDetail(string fullname);
        bool UploadFile(string sourceFile, string pathfolder, string filename, FtpRemoteExists whenExists = FtpRemoteExists.Overwrite, bool createDir = true);
        bool UploadFile(byte[] sourceFile, string pathfolder, string filename, FtpRemoteExists whenExists = FtpRemoteExists.Overwrite, bool createDir = true);
        bool ItemExists(string fullpath);
        void DeleteFile(string fullpath);
        bool CreateDirectory(string fullpath, bool force = false);
        void DeleteDirectory(string fullpath, FtpListOption ftpListOption = FtpListOption.Auto);
        FtpListItem GetObjectInfo(string pathfolder);
        void Dispose();
        public void ClientDisconnect();
        public void ClientConnect();

    }
}
