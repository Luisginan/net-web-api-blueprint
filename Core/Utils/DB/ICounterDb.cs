
namespace Core.Utils.DB;
public interface ICounterDb
{        
    (long, long, DateTime) GetCurrentAndSetNewNextValue(string tableName, string columnName, string partnerCode, string reset, int countdata, int timeOutSecond = 5);


}
