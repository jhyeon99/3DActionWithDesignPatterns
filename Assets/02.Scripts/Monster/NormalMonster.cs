using UnityEngine;

public class NormalMonster : MonsterBase
{
    // 플레이어 방향으로 이동
    protected override void Move()
    {
        MoveTowardTarget();
    }

    protected override void Attack(Player player)
    {
        Debug.Log(player.name);
        player.TakeDamage(_attackDamage);
    }
}