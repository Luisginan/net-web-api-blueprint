using Core.Utils.DB;
using JetBrains.Annotations;
using Core.Utils.FTP.Services;
using Microsoft.Extensions.Logging;
using Moq;
using FluentFTP;

namespace CoreIntegrationTest.Utils.FTP
{
    [TestSubject(typeof(FTPService))]
    public class FTPServiceBaseTest
    {
        protected readonly FTPService _ftpService;


        public FTPServiceBaseTest()
        {
            var mockLogger = new Mock<ILogger<FTPService>>();
            _ftpService = new FTPService(mockLogger.Object);
        }
        protected void mockVerify(
        )
        {
        }

        //FTPS
        protected string FtpsIpAddress => "34.101.81.243";
        protected string FtpsType => "FTPS";
        protected string FtpsPathfolder => "/home/oltest01/ftp_cbas/incoming";
        protected int FtpsPort => 990;
        protected string FtpsUserId => "oltest01";
        protected string FtpsUserPassword => "rnd*911#";


        protected string FtpIpAddress => "localhost";
        protected string FtpType => "FTP";
        protected string FtpPathfolder => "/Testing Aja";
        protected int FtpPort => 21;
        protected string FtpUserId => "NDS";
        protected string FtpUserPassword => "XXX";

    }
}