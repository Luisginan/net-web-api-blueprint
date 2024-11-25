using FluentFTP;
using System.Diagnostics.CodeAnalysis;

namespace Core.Utils.FTP.Models
{
    [ExcludeFromCodeCoverage]
    public class FTPBaseModel
    {
        public string Full_Name { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double File_Size { get; set; } = 0;
        public FtpObjectType File_Type { get; set; }
        public DateTime Created_Date { get; set; } = DateTime.Now;
        public DateTime Last_Modified_Date { get; set; } = DateTime.Now;
    }
    [ExcludeFromCodeCoverage]
    public class FTPDirectoryContent: FTPBaseModel
    {
        public bool Is_Folder { get; set; } = false;
    }
    [ExcludeFromCodeCoverage]
    public class FTPFileDetail: FTPBaseModel
    {
        public byte[] File_Bytes { get; set; } = [];
    }
}
