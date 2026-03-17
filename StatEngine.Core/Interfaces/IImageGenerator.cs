using StatEngine.Core.Models;

namespace StatEngine.Core.Interfaces;

public interface IImageGenerator
{
    Task<string?> GenerateAsync(StatUpdate update);
}
