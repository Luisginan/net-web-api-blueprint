using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Serialization;
using Core.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Core.Utils.Helper
{
    [ExcludeFromCodeCoverage]
    public class ApiClient(ILogger<ApiClient> log,
    IHelperBll helper,
    IOptions<SslCertificateConfig> certConfig) : IApiClient
    {
        public async Task<T?> GetData<T>(string url) where T : class
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(responseBody);
                return result;
            }

            return null;
        }

        protected virtual bool CertificateIsExist()
        {
            var certPath = Path.Combine(certConfig.Value.CertPath, certConfig.Value.CertFilename);
            return File.Exists(certPath);

        }

        protected virtual X509Certificate2 LoadCertFromLocal(string? esbCertPath)
        {
            var file = new MemoryStream(File.ReadAllBytes(esbCertPath!));
            using var reader = new StreamReader(file);
            var pemReader = new PemReader(reader);

            var certEntry = pemReader.ReadObject() as Org.BouncyCastle.X509.X509Certificate;
            if (certEntry == null)
            {
                throw new Exception("Unable to read certificate from PEM file.");
            }

            var cert = new X509Certificate2(certEntry.GetEncoded());

            var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
            if (keyPair != null)
            {
                var privateKey = keyPair.Private;
                var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);

                return cert.CopyWithPrivateKey(DotNetUtilities.ToRSA(keyPair.Private as RsaPrivateCrtKeyParameters));
            }
            else
            {
                return cert;
            }
        }
        public async Task<T?> PostDataNTBXml<T>(string url, string certPath, string contentBody, string mediaType, string logMessageAPI, string logRequestIdApp, string logContractNoApp) where T : class
        {

            var client = Client(certPath);
            string ApiRequestId = Guid.NewGuid().ToString("N").Substring(0, 24);
            using (client)
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Request-ID", ApiRequestId);
                client.DefaultRequestHeaders.Add("CIMB-Timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                Console.WriteLine($"Starting API call to {url} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");

                var content = new StringContent(contentBody, System.Text.Encoding.UTF8, mediaType);
                log.LogInformation("content : " + content);

                var pkLogApi = helper.InsertLogTransactionApi(ApiRequestId, client.DefaultRequestHeaders, "APIExternal", "Core", "Info", url, "POST", logMessageAPI, contentBody, logRequestIdApp, logContractNoApp);

                var response = await client.PostAsync(url, content);
                log.LogInformation("response : " + response.IsSuccessStatusCode);

                helper.UpdateLogTransactionApi(pkLogApi, response.Content.ReadAsStringAsync().Result);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    log.LogInformation("responseBody : " + responseBody);

                    try
                    {
                        var result = DeserializeFromXml<T>(ExtractSoapBody(responseBody));
                        log.LogInformation("result : " + result);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error Deserialize : " + responseBody + "\nError Detail : " + ex.Message);
                    }
                }

                return null;
            }
        }

        private HttpClient Client(string certPath)
        {

            var client = new HttpClient();

            log.LogInformation("CertificateIsExist : " + CertificateIsExist());

            if (CertificateIsExist())
            {
                certPath = Path.Combine(certConfig.Value.CertPath, certConfig.Value.CertFilename);
                var cert = LoadCertFromLocal(certPath);

                var handler = new HttpClientHandler()
                {
                    PreAuthenticate = true,
                    UseDefaultCredentials = false,
                    SslProtocols = SslProtocols.Tls12,
                    ServerCertificateCustomValidationCallback = (message, serverCert, chain, _) =>
                    {
                        return serverCert!.Thumbprint == cert.Thumbprint;
                    }
                };

                client = new HttpClient(handler);

            }

            return client;
        }

        private T DeserializeFromXml<T>(string xml)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute { ElementName = "CIMB_CIFAddOpr", Namespace = "urn:ifxforum-org:XSD:1" });
            using var stringReader = new StringReader(xml);
            return (T)xmlSerializer.Deserialize(stringReader);
        }

        private string ExtractSoapBody(string soapXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapXml);
            var bodyNode = doc.DocumentElement.SelectSingleNode("//*[local-name()='Body']/*");
            return bodyNode.OuterXml;
        }

        public static string? ValidateDateTime(string dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                return null;
            }

            if (DateTime.TryParse(dateTimeString, null, DateTimeStyles.RoundtripKind, out DateTime parsedDateTime))
            {
                if (parsedDateTime == DateTime.MinValue)
                {
                    return null;
                }

                return parsedDateTime.ToString();
            }

            if(dateTimeString.Contains("0001-01-01"))
            {
                return null;
            }

            return null;
        }
    }
}
