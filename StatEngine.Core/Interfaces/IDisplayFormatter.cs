using StatEngine.Core.Models;

namespace StatEngine.Core.Interfaces;

public interface IDisplayFormatter
{
    Task<string> FormatAsync(StatUpdate update);
}
