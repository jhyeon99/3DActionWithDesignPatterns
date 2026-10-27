using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float MoveSpeed = 0f;
    public Vector3 Direction = Vector3.zero;
    public float LifeTime = 0f;

    public float Damage = 0f;
    public int RemainPierce = 0;

    public void FixedUpdate()
    {
        transform.Translate(Direction * MoveSpeed * Time.deltaTime);
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