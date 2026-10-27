using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float MoveSpeed = 0f;
    public Vector3 Direction = Vector3.zero;
    public float LifeTime = 0f;

    public float Damage = 0f;
    public int RemainPierce = 0;

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

    public void FixedUpdate()
    {
        transform.Translate(Direction * MoveSpeed * Time.deltaTime);
    }

    // 수명이 다하면 제거한다. (맞추지 못한 투사체가 영원히 남지 않도록)
    public void Update()
    {
        LifeTime -= Time.deltaTime;
        if (LifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out NormalMonster normalMonster))
        {
            normalMonster.health -= Damage;
            RemainPierce--;
        }

        if (RemainPierce <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (collider.TryGetComponent(out DashMonster dashMonster))
        {
            dashMonster.health -= Damage;
            RemainPierce--;
        }

        if (RemainPierce <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (collider.TryGetComponent(out DebuffMonster debuffMonster))
        {
            debuffMonster.health -= Damage;
            RemainPierce--;
        }

        if (RemainPierce <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}