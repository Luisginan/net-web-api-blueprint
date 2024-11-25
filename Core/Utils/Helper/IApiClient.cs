namespace Core.Utils.Helper;
public interface IApiClient
{
    Task<T?> GetData<T>(string url) where T : class;
    Task<T?> PostDataNTBXml<T>(string url, string certPath, string contentBody, string mediaType, string logMessageAPI, string logRequestIdApp, string logContractNoApp) where T : class;
}