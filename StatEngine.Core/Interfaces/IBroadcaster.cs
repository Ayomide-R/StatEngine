using StatEngine.Core.Models;

namespace StatEngine.Core.Interfaces;

public interface IBroadcaster
{
    Task PostAsync(StatUpdate update);
}
