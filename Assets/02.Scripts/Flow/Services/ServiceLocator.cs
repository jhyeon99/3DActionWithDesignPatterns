using System;
using System.Collections.Generic;

// 서비스 로케이터. 인터페이스 타입을 열쇠로 서비스를 등록하고 꺼내 쓴다.
// 등록은 GameBootstrap에서만 하고, 사용하는 쪽은 Get/TryGet만 쓴다.
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    // T 열쇠로 service를 등록한다. 이미 있으면 교체한다.
    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }

    // 등록된 서비스를 꺼낸다. 등록 전에 쓰면 어떤 서비스가 빠졌는지 알려 주는 예외를 던진다.
    public static T Get<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out object service))
        {
            return (T)service;
        }

        throw new InvalidOperationException($"{typeof(T).Name} 서비스가 등록되지 않았습니다.");
    }

    // 서비스가 없을 수도 있는 곳에서 쓴다. 없으면 false를 돌려준다.
    public static bool TryGet<T>(out T service) where T : class
    {
        if (services.TryGetValue(typeof(T), out object found))
        {
            service = (T)found;
            return true;
        }

        service = null;
        return false;
    }

    // 등록한 서비스를 모두 지운다.
    public static void Clear()
    {
        services.Clear();
    }
}
