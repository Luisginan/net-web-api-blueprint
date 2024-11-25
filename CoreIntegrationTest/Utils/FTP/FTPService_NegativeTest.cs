using Core.Utils.FTP.Services;
using FluentFTP;
using FluentFTP.Exceptions;
using JetBrains.Annotations;
using System;
using System.Security.Authentication;
using System.Text;
using Xunit;

namespace CoreIntegrationTest.Utils.FTP
{
    [TestSubject(typeof(FTPService))]
    public class FTPservice_NegativeTest : FTPServiceBaseTest
    {
        [Fact]
        public void Create_FTPs_InvalidHost_ShouldThrowException()
        {
            Assert.Throws<AggregateException>(() => _ftpService.Create(true, FtpsIpAddress+".111", FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8));
        }
        [Fact]
        public void Create_FTPs_InvalidPort_ShouldThrowException()
        {
            Assert.Throws<TimeoutException>(() => _ftpService.Create(true, FtpsIpAddress, FtpsPort+100, FtpsUserId, FtpsUserPassword, Encoding.UTF8));
        }
        [Fact]
        public void Create_FTPs_InvalidUserId_ShouldThrowException()
        {
            Assert.Throws<FtpAuthenticationException>(() => _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId + ".111", FtpsUserPassword, Encoding.UTF8));
        }
        [Fact]
        public void Create_FTPs_InvalidUserPassword_ShouldThrowException()
        {
            Assert.Throws<FtpAuthenticationException>(() => _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword + ".111", Encoding.UTF8));
        }
        [Fact]
        public void GetFileDetail_FTPs_FileNotExists_ShouldThrowException()
        {

            Assert.Throws<ArgumentException>(() => _ftpService.GetFileDetail($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_zzzz.txt"));
        }


        [Fact]
        public void GetFileDetail_FTPs_ClientNotExists_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _ftpService.GetFileDetail($"{FtpsPathfolder}"));
        }
        [Fact]
        public void GetFileDetail_FTPs_FileNotExists_ShouldReturnNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetFileDetail($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_zzzz.txt");
            Assert.Null(data);
        }

        [Fact]
        public void GetFileDetail_FTPs_ParamIsFolder_ShouldReturnNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetFileDetail($"{FtpsPathfolder}");
            Assert.Null(data);
        }

        [Fact]
        public void GetFileDetail_FTPs_PatchNotExists_ShouldReturnNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetFileDetail($"{FtpsPathfolder}aaa");
            Assert.Null(data);
        }

        [Fact]
        public void GetFileDetail_FTP_FileNotExists_ShouldReturnNull()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var data = _ftpService.GetFileDetail($"{FtpPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_zzzz.txt");
            Assert.Null(data);
        }
        [Fact]
        public void GetDirectoryContent_FTPs_ClientNotExists_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _ftpService.GetDirectoryContent($"{FtpsPathfolder}"));
        }
        [Fact]
        public void GetDirectoryContent_FTPs_PatchNotExists_ShouldReturnNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetDirectoryContent($"{FtpsPathfolder}aaa");
            Assert.Empty(data);
        }

        [Fact]
        public void GetObjectInfo_FTPs_ClientNotExists_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _ftpService.GetObjectInfo($"{FtpsPathfolder}"));
        }
        [Fact]
        public void GetObjectInfo_FTP_NotExists_ShouldNull()
        {
            _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8, FtpEncryptionMode.None);
            var data = _ftpService.GetObjectInfo($"{FtpPathfolder}aaaaa");
            Assert.Null(data);
        }
        [Fact]
        public void GetObjectInfo_FTPs_NotExists_ShouldNull()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.GetObjectInfo($"{FtpsPathfolder}aaaaa");
            Assert.Null(data);
        }
        [Fact]
        public void UploadFile_FTPs_ClientNotExists_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _ftpService.UploadFile($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_0.txt", $"{FtpsPathfolder}", $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_0.txt"));
        }
        

        [Fact]
        public void UploadFile_FTPs_SourceNotExists_ShouldReturnFalse()
        {
            _ftpService.Create(true, FtpsIpAddress, FtpsPort, FtpsUserId, FtpsUserPassword, Encoding.UTF8);
            var data = _ftpService.UploadFile($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_zzzzz.txt", $"{FtpsPathfolder}", $"BatchID{DateTime.Now.ToString("yyyyMMdd")}_0.txt");
            Assert.False(data);
        }

        [Fact]
        public void ItemExists_FTPs_ClientNotExists_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _ftpService.ItemExists($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_0.txt"));
        }
        [Fact]
        public void DeleteFile_FTPs_ClientNotExists_ShouldThrowException()
        {
            Assert.Throws<ArgumentException>(() => _ftpService.DeleteFile($"{FtpsPathfolder}/BatchID{DateTime.Now.ToString("yyyyMMdd")}_0.txt"));
        }


        [Fact]
        public void Create_FTP_InvalidEncryptionMode_ShouldThrowException()
        {
            Assert.Throws<AuthenticationException>(() => _ftpService.Create(false, FtpIpAddress, FtpPort, FtpUserId, FtpUserPassword, Encoding.UTF8));
        }


    }
}