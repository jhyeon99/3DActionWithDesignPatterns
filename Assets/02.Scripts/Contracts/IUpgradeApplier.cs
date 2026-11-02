// 증강 적용 계약. GameManager는 증강을 고르게 하기만 하고, 실제 적용은 이 계약의 구현이 맡는다.
public interface IUpgradeApplier
{
    // 선택한 증강을 플레이어에게 적용한다.
    void Apply(UpgradeType upgradeType);

    // 새 런을 시작할 때 증강으로 올라간 능력치를 처음으로 되돌린다.
    void ResetStats();
}
