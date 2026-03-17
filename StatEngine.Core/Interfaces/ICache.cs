namespace StatEngine.Core.Interfaces;

public interface ICache
{
    Task<bool> IsNewAsync(string id);
    Task MarkAsPostedAsync(string id);
}
