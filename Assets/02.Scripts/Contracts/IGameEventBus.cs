using System;

// 게임 이벤트 버스 계약.
// 발행하는 쪽과 구독하는 쪽이 서로를 모른 채 이벤트 타입만으로 연결된다.
// 구독한 쪽은 반드시 OnDisable/OnDestroy에서 Unsubscribe 해야 한다.
public interface IGameEventBus
{
    // T 이벤트가 발행될 때 handler를 호출한다. 같은 handler를 두 번 등록해도 한 번만 호출된다.
    void Subscribe<T>(Action<T> handler) where T : struct;

    // 등록한 handler를 해제한다. 등록되지 않은 handler라면 아무 일도 하지 않는다.
    void Unsubscribe<T>(Action<T> handler) where T : struct;

    // T 이벤트를 구독자 전원에게 전달한다. 구독자가 없으면 아무 일도 하지 않는다.
    void Publish<T>(T gameEvent) where T : struct;
}
