// 경험치 서비스 계약. 경험치와 레벨을 한 곳에서 관리한다.
// 경험치가 바뀌면 ExperienceChangedEvent, 레벨이 오르면 LevelUpEvent가 발행되므로
// 화면(HUD)과 증강 선택은 이 이벤트만 구독하면 된다.
public interface IExperienceService
{
    // 현재 레벨
    int Level { get; }

    // 현재 경험치
    float Experience { get; }

    // 다음 레벨까지 필요한 경험치
    float MaxExperience { get; }

    // 경험치를 더한다. 가득 차면 레벨이 오르고, 한 번에 여러 레벨이 오를 수도 있다.
    void Add(float amount);

    // 경험치는 그대로 두고 레벨만 한 단계 올린다. (테스트나 보상용)
    void LevelUp();

    // 새 게임을 시작할 때 레벨 1, 경험치 0으로 되돌린다.
    void Reset();
}
