using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DanmakuEngine.DependencyInjection;

internal class ScopedEngine
{
    private readonly ServiceProviderBase _provider;
    private IDictionary<Type, ServiceAccessor> _accessors;

    internal readonly IDictionary<Type, object> _cache = new Dictionary<Type, object>();

    internal ScopedEngine(ServiceProviderBase provider)
    {
        _provider = provider;
        _accessors = provider._accessors;
    }

    internal void UpdateCache<T>(T instance)
    {
        if (instance == null)
        {
            _cache.Remove(typeof(T));

            return;
        }

        if (_cache.TryAdd(typeof(T), instance))
            return;

        _cache[typeof(T)] = instance;
    }

    internal bool HasAccessor(Type type)
        =>_accessors.ContainsKey(type);

    internal bool HasCached(Type type)
        =>_cache.ContainsKey(type);

    internal bool CanResolve(Type type)
        => HasAccessor(type) || HasCached(type);

    internal object Resolve(Type type)
    {
        if (_cache.TryGetValue(type, out var instance))
        {
            return instance;
        }

        var accessor = _accessors[type];
        instance = accessor.Create(_provider);

        Debug.Assert(instance != null, $"Failed to create instance of {type}");

        _cache[type] = instance;
        return instance;
    }

    internal bool TryResolve(Type type, [NotNullWhen(true)] out object? instance)
    {
        if (_cache.TryGetValue(type, out instance))
        {
            return true;
        }

        if (_accessors.TryGetValue(type, out var accessor))
        {
            instance = accessor.Create(_provider);
            Debug.Assert(instance != null, $"Failed to create instance of {type}");

            _cache[type] = instance;
            return true;
        }

        return false;
    }
}