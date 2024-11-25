using Core.Utils.FTP.Services;
using FluentFTP;
using JetBrains.Annotations;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace CoreIntegrationTest.Utils.FTP
{
    [TestSubject(typeof(FTPService))]
    public class FTPservice_PositiveTest : FTPServiceBaseTest
    {
        [Fact]
        public void Create__FTPs_Valid_Data_Success()
        {
            // Arrange

            //Action
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
        }

        [Fact]
        public void UploadFile_FTPs_SourceExist_FileAdded()
        {
            string newFile = $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt";
            string content = "";
            string reffNo = "Reff" + Convert.ToString(Guid.NewGuid());
            content += reffNo + "|06|I|" + "test";
            content += Environment.NewLine;
            File.AppendAllText(newFile, content);

            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.UploadFile(newFile, $"{FtpsPathfolder}", newFile);
            File.Delete(newFile);
            Assert.True(data);
        }
        [Fact]
        public void UploadFile_FTPs_Byte_FileAdded()
        {
            string newFile = $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt";
            string content = "";
            string reffNo = "Reff" + Convert.ToString(Guid.NewGuid());
            content += reffNo + "|06|I|" + "test";
            content += Environment.NewLine;
            File.AppendAllText(newFile, content);

            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.UploadFile(File.ReadAllBytes(newFile), $"{FtpsPathfolder}", newFile);
            File.Delete(newFile);
            Assert.True(data);
        }
        [Fact]
        public void GetFileDetail_FTPs_FileExists_ShouldReturnNotNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetFileDetail($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt"); //run upload file firsst
            Assert.NotNull(data);
        }
        [Fact]
        public void GetDirectoryContent_FTPs_DataExists_ShouldReturnNotEmptyList()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetDirectoryContent($"{FtpsPathfolder}");
            Assert.NotEmpty(data);
        }
        [Fact]
        public void GetObjectInfo_FTP_Exists_ShouldReturnNotNull()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var data = _ftpService.GetObjectInfo($"{FtpPathfolder}");
            Assert.NotNull(data);
        }
        [Fact]
        public void GetObjectInfo_FTPs_Exists_ShouldReturnNotNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetObjectInfo($"{FtpsPathfolder}");
            Assert.NotNull(data);
        }
        [Fact]
        public void ItemExists_FTPs_File_Success_ShouldReturnTrue()
        {
            string newFile = $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt";
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.ItemExists($"{FtpsPathfolder}/{newFile}");
            Assert.True(data);
        }
        [Fact]
        public void ItemExists_FTPs_Folder_Success_ShouldReturnTrue()
        {
            string newFile = $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt";
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.ItemExists($"{FtpsPathfolder}");
            Assert.True(data);
        }
        [Fact]
        public void DeleteFile_FTPs_FileExists_FileDeleted()
        {
            string newFile = $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt";
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            _ftpService.DeleteFile($"{FtpsPathfolder}/{newFile}");


        }
        [Fact]
        public void Dispose_FTPs_Success()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            _ftpService.Dispose();
        }
        [Fact]
        public void GetFTPClient_FTP_Success()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            FtpClient client = _ftpService.GetFTPClient();
            Assert.NotNull(client);
        }
        [Fact]
        public void GetWorkingDirectory_FTP_Success()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            string workDir = _ftpService.GetWorkingDirectory();
            Assert.False(string.IsNullOrWhiteSpace(workDir));
        }
        [Fact]
        public void UploadFile_FTP_SourceExist_FileAdded()
        {
            string newFile = $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt";
            string content = "";
            string reffNo = "Reff" + Convert.ToString(Guid.NewGuid());
            content += reffNo + "|06|I|" + "test";
            content += Environment.NewLine;
            File.AppendAllText(newFile, content);

            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var data = _ftpService.UploadFile(newFile, $"{FtpPathfolder}", newFile);
            File.Delete(newFile);
            Assert.True(data);
        }

        [Fact]
        public void GetFileDetail_FTP_FileExists_ShouldReturnNotNull()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var data = _ftpService.GetFileDetail($"{FtpPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_zz.txt"); //run upload file firsst
            Assert.NotNull(data);
        }
        [Fact]
        public void CreateDirectory_FTP_FolderExists_ShouldReturnFalseAndNotNulLWhenGetObject()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var success = _ftpService.CreateDirectory($"{FtpPathfolder}/testaja", true);
            var data = _ftpService.GetObjectInfo($"{FtpPathfolder}/testaja");
            Assert.False(success);
            Assert.NotNull(data);
        }
        [Fact]
        public void CreateDirectory_FTPs_FolderExists_ShouldReturnFalseAndNotNulLWhenGetObject()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var success = _ftpService.CreateDirectory($"{FtpsPathfolder}/testaja", true);
            var data = _ftpService.GetObjectInfo($"{FtpsPathfolder}/testaja");
            Assert.False(success);
            Assert.NotNull(data);
        }
        [Fact]
        public void CreateDirectory_FTP_FolderNotExists_ShouldReturnTrueAndNotNulLWhenGetObject()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var success = _ftpService.CreateDirectory($"{FtpPathfolder}/testaja", true);
            var data = _ftpService.GetObjectInfo($"{FtpPathfolder}/testaja");
            Assert.True(success);
            Assert.NotNull(data);
        }
        [Fact]
        public void CreateDirectory_FTPs_FolderNotExists_ShouldReturnTrueAndNotNulLWhenGetObject()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var success = _ftpService.CreateDirectory($"{FtpsPathfolder}/testaja", true);
            var data = _ftpService.GetObjectInfo($"{FtpsPathfolder}/testaja");
            Assert.True(success);
            Assert.NotNull(data);
        }
        [Fact]
        public void DeleteDirectory_FTP_Exists_ShouldReturnNullWhenGetObject()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            _ftpService.DeleteDirectory($"{FtpPathfolder}/testaja");
            var data = _ftpService.GetObjectInfo($"{FtpPathfolder}/testaja");
            Assert.Null(data);
        }
        [Fact]
        public void DeleteDirectory_FTPs_Exists_ShouldReturnNullWhenGetObject()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            _ftpService.DeleteDirectory($"{FtpsPathfolder}/testaja");
            var data = _ftpService.GetObjectInfo($"{FtpsPathfolder}/testaja");
            Assert.Null(data);
        }
    }
}