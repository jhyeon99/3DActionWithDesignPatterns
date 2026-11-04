using System.Collections;
using UnityEngine;

public class DebuffMonster : MonsterBase
{
    [SerializeField] private float _castTime = 2f;
    [SerializeField] private float _castCoolTime = 6f;
    private float _elapsed = 0f;

    private Coroutine _debuffCoroutine;

    // 쿨타임마다 디버프 시전, 시전 중이 아니면 플레이어 방향으로 이동
    protected override void Move()
    {
        transform.forward = _direction.normalized;

        if (_elapsed >= _castCoolTime)
        {
            _elapsed = 0f;
            _debuffCoroutine = StartCoroutine(Debuff());
        }

        if (_debuffCoroutine == null)
        {
            _elapsed += Time.deltaTime;
            MoveTowardTarget();
        }
    }

    protected override void Attack(Player player)
    {
        Debug.Log(player.name);
        player.TakeDamage(_attackDamage);
    }

    private IEnumerator Debuff()
    {
        int randomDebuff = Random.Range(1, 4);

        yield return new WaitForSeconds(_castTime);

        switch (randomDebuff)
        {
            case 1: // 이속 감소
                Debug.Log("이감!");
                break;
            case 2: // 공속 감소
                Debug.Log("공감!");
                break;
            case 3: // 기절
                Debug.Log("기절!");
                break;
        }

        _elapsed = 0f;
        _debuffCoroutine = null;
    }
}