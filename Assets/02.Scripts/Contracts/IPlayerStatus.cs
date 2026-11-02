// 플레이어 상태 계약. GameManager는 Player 클래스를 직접 알지 않고 이 계약만 본다.
// Player 담당(송재욱)이 이 계약을 직접 구현하면 Legacy 구현을 지울 수 있다.
public interface IPlayerStatus
{
    // 플레이어가 죽었는가.
    bool IsDead { get; }

    // 새 런을 시작할 때 체력과 상태를 처음으로 되돌린다.
    void ResetForNewRun();
}
