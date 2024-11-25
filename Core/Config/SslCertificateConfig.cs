using System.Diagnostics.CodeAnalysis;

namespace Core.Config;

[ExcludeFromCodeCoverage]
public class SslCertificateConfig
{
    public string CertPath { get; set; } = "";
    public string CertFilename { get; set; } = "";
    public string Type { get; set; } = "";
}