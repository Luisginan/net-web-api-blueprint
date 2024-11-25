namespace Core.Utils.Security;

public interface IVault
{
    T RevealSecret<T>(T config);
}
