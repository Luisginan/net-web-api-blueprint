namespace Core.Utils.Helper;
public interface IHelperBll
{
    void LogInfo(object objectMessage);
    void LogError(object message, Exception exception);
    long InsertLogTransactionApi(string apiRequestId, object header, string tag, string type, string level, string endPoint, string apiMethod, string message, object requestBody, string requestIdApp, string contractNoApp);
    void UpdateLogTransactionApi(long logId, object responseBody);
    T ConvertXMLtoClass<T>(string xml);

}
