using UnityEngine;

// IScoreService 구현. MonoBehaviour가 아닌 순수 C# 클래스다.
// 점수가 바뀌면 이벤트 버스로 ScoreChangedEvent를 발행하고, 화면 갱신은 구독한 쪽이 한다.
public sealed class ScoreService : IScoreService
{
    private readonly IGameEventBus bus;

    public float Score { get; private set; }

    public int KillCount { get; private set; }

    public ScoreService(IGameEventBus bus)
    {
        this.bus = bus;
    }

    public void AddKill(float scoreValue)
    {
        KillCount++;
        Add(scoreValue);
    }

    public void Add(float value)
    {
        Score += value;
        bus.Publish(new ScoreChangedEvent(Score));
    }

    public void Subtract(float value)
    {
        Score = Mathf.Max(0f, Score - value);
        bus.Publish(new ScoreChangedEvent(Score));
    }

    public void Reset()
    {
        Score = 0f;
        KillCount = 0;
        bus.Publish(new ScoreChangedEvent(Score));
    }
}
