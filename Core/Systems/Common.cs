using System.Diagnostics.CodeAnalysis;

namespace Core.Systems;
[ExcludeFromCodeCoverage]
public static class Common
{
    public static string GetHttpFlavour(string protocol)
    {
        if (HttpProtocol.IsHttp10(protocol))
        {
            return "1.0";
        }

        if (HttpProtocol.IsHttp11(protocol))
        {
            return "1.1";
        }

        if (HttpProtocol.IsHttp2(protocol))
        {
            return "2.0";
        }

        if (HttpProtocol.IsHttp3(protocol))
        {
            return "3.0";
        }

        throw new InvalidOperationException($"Protocol {protocol} not recognised.");
    }
}