using UnityEngine;

// IUpgradeApplier의 기본(Legacy) 구현. 기존 GameManager.ApplyUpgrade의 switch를 그대로 옮겼다.
// Player, PlayerFire, PlayerMove 세 곳을 직접 아는 것은 이 파일뿐이다.
public sealed class LegacyUpgradeApplier : IUpgradeApplier
{
    private Player player;
    private PlayerFire playerFire;
    private PlayerMove playerMove;

    public void Apply(UpgradeType upgradeType)
    {
        Resolve();

        switch (upgradeType)
        {
            case UpgradeType.AttackSpeed:
                if (playerFire != null) playerFire.UpgradeAttackSpeed(0.15f);
                Debug.Log("공격 속도 증가");
                break;

            case UpgradeType.Damage:
                if (playerFire != null) playerFire.UpgradeDamage(0.2f);
                Debug.Log("공격력 증가");
                break;

            case UpgradeType.MoveSpeed:
                if (playerMove != null) playerMove.UpgradeMoveSpeed(0.15f);
                Debug.Log("이동 속도 증가");
                break;

            case UpgradeType.Heal:
                if (player != null) player.UpgradeHeal(30f);
                Debug.Log("체력 회복");
                break;

            case UpgradeType.HealthUp:
                if (player != null) player.UpgradeMaxHp(10f);
                Debug.Log("최대 체력 증가");
                break;

            case UpgradeType.DefenseUp:
                if (player != null) player.UpgradeDefense(10f);
                Debug.Log("방어력 증가");
                break;
        }
    }

    public void ResetStats()
    {
        Resolve();

        if (playerFire != null) playerFire.ResetStats();
        if (playerMove != null) playerMove.ResetStats();
    }

    // 아직 못 찾았거나 파괴된 컴포넌트만 다시 찾는다.
    private void Resolve()
    {
        if (player == null) player = Object.FindFirstObjectByType<Player>();
        if (playerFire == null) playerFire = Object.FindFirstObjectByType<PlayerFire>();
        if (playerMove == null) playerMove = Object.FindFirstObjectByType<PlayerMove>();
    }
}
