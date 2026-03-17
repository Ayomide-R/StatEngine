using StatEngine.Core.Models;

namespace StatEngine.Core.Interfaces;

public interface IStatProvider
{
    string Name { get; }
    Task<StatUpdate> GetLatestAsync();
}
