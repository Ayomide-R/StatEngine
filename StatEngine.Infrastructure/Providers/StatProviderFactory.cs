using StatEngine.Core.Interfaces;

namespace StatEngine.Infrastructure.Providers;

public class StatProviderFactory
{
    private readonly IEnumerable<IStatProvider> _providers;
    private readonly Random _random = new();

    public StatProviderFactory(IEnumerable<IStatProvider> providers)
    {
        _providers = providers;
    }

    public IStatProvider GetRandomProvider()
    {
        var providersList = _providers.ToList();
        if (!providersList.Any())
        {
            throw new InvalidOperationException("No stat providers registered.");
        }
        return providersList[_random.Next(providersList.Count)];
    }
}
