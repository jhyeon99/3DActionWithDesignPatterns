using System.Collections;
using UnityEngine;

public class DashMonster : MonsterBase
{
    [SerializeField] private float _dashSpeed = 5f;
    [SerializeField] private float _dashRange = 10f;
    [SerializeField] private float _dashRecognizeRange = 5f;
    private Coroutine _dashCoroutine;

    // 돌진 범위 안이면 돌진, 아니면 플레이어 방향으로 이동
    protected override void Move()
    {
        // 돌진 중에는 이동하지 않도록
        if (_dashCoroutine != null)
        {
            return;
        }

        // 플레이어와의 거리가 돌진 인식 범위보다 작아지면 돌진 수행
        if (_direction.magnitude < _dashRange)
        {
            Vector3 moveDirection = _direction.normalized;
            moveDirection.y = 0f;
            _dashCoroutine = StartCoroutine(Dash(moveDirection, _direction.magnitude));
            return;
        }

        MoveTowardTarget();
    }

    protected override void Attack(Player player)
    {
        Debug.Log("플레이어 공격!");
        player.TakeDamage(_attackDamage);
    }

    private IEnumerator Dash(Vector3 direction, float distance)
    {
        Debug.Log("저돌맹진!!");

        Vector3 endPosition = transform.position + direction * distance;

        yield return new WaitForSeconds(2f);

        // 막혀서 도착하지 못해도 돌진이 끝나도록 최대 돌진 시간 제한 (여유 1.5배)
        float maxDashTime = distance / _dashSpeed * 1.5f;
        float dashElapsed = 0f;

        // 돌진 거리만큼 이동하거나 최대 돌진 시간이 지나면 종료하도록
        while (Vector3.Distance(transform.position, endPosition) > 0.2f && dashElapsed < maxDashTime)
        {
            dashElapsed += Time.deltaTime;
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPosition,
                _dashSpeed * Time.deltaTime
            );
            yield return null;
        }

        Debug.Log("저돌맹진 종료!!");

        _dashCoroutine = null;
    }
}