using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;
using Stl.Interception.Internal;

namespace Stl.Interception;

public static class Proxies
{
    private static readonly ConcurrentDictionary<Type, Type?> Cache = new();

    // New

#pragma warning disable IL2072
    public static TType New<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TType>
        (Interceptor interceptor, object? proxyTarget = null)
        where TType : class, IRequiresAsyncProxy
    {
        var proxy = (TType)GetProxyType(typeof(TType)).CreateInstance();
        return interceptor.BindTo(proxy, proxyTarget);
    }

    public static TType New<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TType, T1>
        (T1 arg1, Interceptor interceptor, object? proxyTarget = null)
        where TType : class, IRequiresAsyncProxy
    {
        var proxy = (TType)GetProxyType(typeof(TType)).CreateInstance(arg1);
        return interceptor.BindTo(proxy, proxyTarget);
    }

    public static TType New<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TType, T1, T2>
        (T1 arg1, T2 arg2, Interceptor interceptor, object? proxyTarget = null)
        where TType : class, IRequiresAsyncProxy
    {
        var proxy = (TType)GetProxyType(typeof(TType)).CreateInstance(arg1, arg2);
        return interceptor.BindTo(proxy, proxyTarget);
    }

    public static TType New<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TType, T1, T2, T3>
        (T1 arg1, T2 arg2, T3 arg3, Interceptor interceptor, object? proxyTarget = null)
        where TType : class, IRequiresAsyncProxy
    {
        var proxy = (TType)GetProxyType(typeof(TType)).CreateInstance(arg1, arg2, arg3);
        return interceptor.BindTo(proxy, proxyTarget);
    }

    public static TType New<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TType, T1, T2, T3, T4>
        (T1 arg1, T2 arg2, T3 arg3, T4 arg4, Interceptor interceptor, object? proxyTarget = null)
        where TType : class, IRequiresAsyncProxy
    {
        var proxy = (TType)GetProxyType(typeof(TType)).CreateInstance(arg1, arg2, arg3, arg4);
        return interceptor.BindTo(proxy, proxyTarget);
    }

    public static IProxy New(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        Interceptor interceptor, object? proxyTarget = null)
    {
        var proxy = (IProxy)GetProxyType(type).CreateInstance();
        return InterceptorExt.BindTo(interceptor, proxy, proxyTarget);
    }

    public static IProxy New<T1>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        T1 arg1, Interceptor interceptor, object? proxyTarget = null)
    {
        var proxy = (IProxy)GetProxyType(type).CreateInstance(arg1);
        return InterceptorExt.BindTo(interceptor, proxy, proxyTarget);
    }

    public static IProxy New<T1, T2>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        T1 arg1, T2 arg2, Interceptor interceptor, object? proxyTarget = null)
    {
        var proxy = (IProxy)GetProxyType(type).CreateInstance(arg1, arg2);
        return InterceptorExt.BindTo(interceptor, proxy, proxyTarget);
    }

    public static IProxy New<T1, T2, T3>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        T1 arg1, T2 arg2, T3 arg3, Interceptor interceptor, object? proxyTarget = null)
    {
        var proxy = (IProxy)GetProxyType(type).CreateInstance(arg1, arg2, arg3);
        return InterceptorExt.BindTo(interceptor, proxy, proxyTarget);
    }

    public static IProxy New<T1, T2, T3, T4>(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, Interceptor interceptor, object? proxyTarget = null)
    {
        var proxy = (IProxy)GetProxyType(type).CreateInstance(arg1, arg2, arg3, arg4);
        return InterceptorExt.BindTo(interceptor, proxy, proxyTarget);
    }
#pragma warning restore IL2072

    // GetProxyType

#pragma warning disable IL2026, IL2055
    public static Type GetProxyType<TType>()
        where TType : class, IRequiresAsyncProxy
        => GetProxyType(typeof(TType));

    public static Type GetProxyType(Type type)
        => TryGetProxyType(type) ?? throw Errors.NoProxyType(type);

    public static Type? TryGetProxyType(Type type)
        => Cache.GetOrAdd(type, static type1 => {
            if (type1.IsConstructedGenericType) {
                var genericType = TryGetProxyType(type1.GetGenericTypeDefinition());
                return genericType?.MakeGenericType(type1.GenericTypeArguments);
            }

            var name = type1.Name;
            var namePrefix = name;
            var nameSuffix = "";
            if (type1.IsGenericTypeDefinition) {
                var backTrickIndex = name.IndexOf('`', StringComparison.Ordinal);
                if (backTrickIndex < 0)
                    return null; // Weird case, shouldn't happen

                namePrefix = name[..backTrickIndex];
                nameSuffix = name[backTrickIndex..];
            }
            var proxyTypeName = ZString.Concat(
                type1.Namespace,
                ".StlInterceptionProxies.",
                namePrefix,
                "Proxy",
                nameSuffix);
            return type1.Assembly.GetType(proxyTypeName);
        });
#pragma warning restore IL2026, IL2055
}
