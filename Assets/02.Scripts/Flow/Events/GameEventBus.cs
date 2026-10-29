using System;
using System.Collections.Generic;
using UnityEngine;

// 이벤트 버스 구현. MonoBehaviour가 아닌 순수 C# 클래스다.
// 이벤트 타입별로 구독자 목록(델리게이트)을 하나씩 들고 있고, Publish는 가비지를 만들지 않는다.
public sealed class GameEventBus : IGameEventBus
{
    private readonly Dictionary<Type, Delegate> handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        if (handler == null)
        {
            return;
        }

        Type key = typeof(T);
        handlers.TryGetValue(key, out Delegate existing);

        // 같은 handler가 이미 있으면 두 번 호출되지 않도록 무시한다.
        if (existing != null && Array.IndexOf(existing.GetInvocationList(), handler) >= 0)
        {
            return;
        }

        handlers[key] = Delegate.Combine(existing, handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : struct
    {
        if (handler == null)
        {
            return;
        }

        Type key = typeof(T);
        if (!handlers.TryGetValue(key, out Delegate existing))
        {
            return;
        }

        Delegate remaining = Delegate.Remove(existing, handler);
        if (remaining == null)
        {
            handlers.Remove(key);
        }
        else
        {
            handlers[key] = remaining;
        }
    }

    public void Publish<T>(T gameEvent) where T : struct
    {
        if (!handlers.TryGetValue(typeof(T), out Delegate existing))
        {
            return;
        }

        // 델리게이트는 불변이라, 구독자가 호출 도중 Subscribe/Unsubscribe 해도 이번 발행에는 영향이 없다.
        try
        {
            ((Action<T>)existing).Invoke(gameEvent);
        }
        catch (Exception exception)
        {
            // 한 구독자의 오류가 게임 전체를 멈추지 않게 한다. (이후 구독자 호출은 건너뛴다)
            Debug.LogException(exception);
        }
    }
}
