// 점수 서비스 계약. 점수와 처치 수를 한 곳에서 관리한다.
// 점수가 바뀔 때마다 ScoreChangedEvent가 발행되므로, 화면(HUD)은 이 이벤트만 구독하면 된다.
public interface IScoreService
{
    // 현재 점수
    float Score { get; }

    // 처치한 적의 수
    int KillCount { get; }

    // 적 한 마리를 처치했다. 처치 수를 올리고 점수를 더한다.
    void AddKill(float scoreValue);

    // 점수를 더한다.
    void Add(float value);

    // 점수를 뺀다. 0 아래로는 내려가지 않는다.
    void Subtract(float value);

    // 새 게임을 시작할 때 점수와 처치 수를 0으로 되돌린다.
    void Reset();
}
