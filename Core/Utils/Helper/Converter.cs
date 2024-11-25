using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Core.Utils.Helper
{
    public static class Converter
    {
        public static string HeadersToJson(HttpHeaders headers)
        {
            if (headers != null)
            {
                var headersDict = new Dictionary<string, IEnumerable<string>>();

                foreach (var header in headers)
                {
                    headersDict[header.Key] = header.Value;
                }

                return JsonConvert.SerializeObject(headersDict, Formatting.Indented);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string HeadersToJson(HttpRequestHeaders headers)
        {
            if(headers != null)
            {
                var headersDict = new Dictionary<string, IEnumerable<string>>();

                foreach (var header in headers)
                {
                    headersDict[header.Key] = header.Value;
                }

                return JsonConvert.SerializeObject(headersDict, Formatting.Indented);
            }
            else
            {
                return string.Empty;
            }
           
        }
    }
}
