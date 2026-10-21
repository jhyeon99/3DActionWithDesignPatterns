using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    public float FireRate = 0.1f;
    public float FireTimer = 0f;
    public GameObject BulletPrefab;

    public void Update()
    {
        FireTimer += Time.deltaTime;
        if (FireTimer >= FireRate)
        {
            Fire();
            FireTimer -= FireRate;
        }
    }

    public void Fire()
    {
        var normalMonsters = FindObjectsByType<NormalMonster>(FindObjectsSortMode.None);
        var dashMonsters = FindObjectsByType<DashMonster>(FindObjectsSortMode.None);
        var debuffMonsters = FindObjectsByType<DebuffMonster>(FindObjectsSortMode.None);

        float minDistance = float.MaxValue;
        var targetPos = Vector3.zero;

        foreach (var monster in normalMonsters)
        {
            if (Mathf.Abs(monster.transform.position.x - transform.position.x) +
                Mathf.Abs(monster.transform.position.z - transform.position.z) < minDistance)
            {
                minDistance = Mathf.Abs(monster.transform.position.x - transform.position.x) +
                              Mathf.Abs(monster.transform.position.z - transform.position.z);
                targetPos = monster.transform.position;
            }
        }

        foreach (var monster in dashMonsters)
        {
            if (Mathf.Abs(monster.transform.position.x - transform.position.x) +
                Mathf.Abs(monster.transform.position.z - transform.position.z) < minDistance)
            {
                minDistance = Mathf.Abs(monster.transform.position.x - transform.position.x) +
                              Mathf.Abs(monster.transform.position.z - transform.position.z);
                targetPos = monster.transform.position;
            }
        }

        foreach (var monster in debuffMonsters)
        {
            if (Mathf.Abs(monster.transform.position.x - transform.position.x) +
                Mathf.Abs(monster.transform.position.z - transform.position.z) < minDistance)
            {
                minDistance = Mathf.Abs(monster.transform.position.x - transform.position.x) +
                              Mathf.Abs(monster.transform.position.z - transform.position.z);
                targetPos = monster.transform.position;
            }
        }

        Vector3 targetDirection = Vector3.zero;
        targetDirection = minDistance == float.MaxValue ? transform.forward : targetPos - transform.position;
        targetDirection.Normalize();
    }
}