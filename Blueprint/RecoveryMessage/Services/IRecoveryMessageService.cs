using Core.Utils.DB;

namespace Blueprint.RecoveryMessage.Services;

public interface IRecoveryMessageService
{
    Task<List<Message<string>>> GetRecoveryMessages();
    Task UpdateMessage(string key, string message);
    void ResendMessage(string key);
}