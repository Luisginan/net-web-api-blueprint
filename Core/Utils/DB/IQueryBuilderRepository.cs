using NawaDataDAL.Common;

namespace Core.Utils.DB
{
    public interface IQueryBuilderRepository
    {
        void Register(NawaDAO.DBType dbType, string path);
        string GetQuery(string key);
        string GetQuery(string key, string connectionKey);
        string GetQuery(string key, NawaDAO.DBType dbType);
    }

}
