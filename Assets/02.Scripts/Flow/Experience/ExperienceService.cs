// IExperienceService 구현. MonoBehaviour가 아닌 순수 C# 클래스다.
// 레벨이 오르거나 경험치가 바뀌면 이벤트 버스로 알리고, 화면 갱신이나 증강 선택은 구독한 쪽이 한다.
public sealed class ExperienceService : IExperienceService
{
    // 레벨 1에서 다음 레벨까지 필요한 경험치
    private const float InitialMaxExperience = 100f;

    // 레벨이 오를 때마다 필요한 경험치가 늘어나는 배율
    private const float MaxExperienceGrowth = 1.1f;

    private readonly IGameEventBus bus;

    public int Level { get; private set; } = 1;

    public float Experience { get; private set; }

    public float MaxExperience { get; private set; } = InitialMaxExperience;

    public ExperienceService(IGameEventBus bus)
    {
        this.bus = bus;
    }

    public void Add(float amount)
    {
        Experience += amount;
        if (MaxExperience <= 0f)
        {
            return;
        }

        while (Experience >= MaxExperience)
        {
            Experience -= MaxExperience;
            RaiseLevel();
        }

        PublishChanged();
    }

    public void LevelUp()
    {
        RaiseLevel();
        PublishChanged();
    }

    public void Reset()
    {
        Level = 1;
        Experience = 0f;
        MaxExperience = InitialMaxExperience;
        PublishChanged();
    }

    private void RaiseLevel()
    {
        Level++;
        MaxExperience *= MaxExperienceGrowth;
        bus.Publish(new LevelUpEvent(Level));
    }

    private void PublishChanged()
    {
        bus.Publish(new ExperienceChangedEvent(Experience, MaxExperience));
    }
}
