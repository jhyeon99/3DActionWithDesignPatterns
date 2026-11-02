using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    private float _moveSpeed = 0f;
    private Vector3 _direction = Vector3.zero;
    private float _lifeTime = 0f;

    private float _damage = 0f;
    private int _remainPierce = 0;

    // 발사하는 쪽이 생성 직후 한 번 호출해서 값을 넣는다.
    public void Init(float moveSpeed, Vector3 direction, float lifeTime, float damage, int remainPierce)
    {
        _moveSpeed = moveSpeed;
        _direction = direction;
        _lifeTime = lifeTime;
        _damage = damage;
        _remainPierce = remainPierce;
    }

    private void Awake()
    {
        DebugStats.OnCreated(StatKind.Bullet);

        // 충돌 시 OnTriggerEnter로 처리하므로 트리거여야 한다. (몬스터의 Rigidbody와 짝을 이룬다)
        if (TryGetComponent(out Collider bulletCollider))
        {
            bulletCollider.isTrigger = true;
        }
    }

    private void OnDestroy()
    {
        DebugStats.OnDestroyed(StatKind.Bullet);
    }

    private void FixedUpdate()
    {
        transform.Translate(_direction * _moveSpeed * Time.deltaTime);
    }

    // 수명이 다하면 제거한다. (맞추지 못한 투사체가 영원히 남지 않도록)
    private void Update()
    {
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out NormalMonster normalMonster))
        {
            normalMonster.TakeDamage(_damage);
            _remainPierce--;
        }

        if (_remainPierce <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (collider.TryGetComponent(out DashMonster dashMonster))
        {
            dashMonster.TakeDamage(_damage);
            _remainPierce--;
        }

        if (_remainPierce <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (collider.TryGetComponent(out DebuffMonster debuffMonster))
        {
            debuffMonster.TakeDamage(_damage);
            _remainPierce--;
        }

        if (_remainPierce <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}