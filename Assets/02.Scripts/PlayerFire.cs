using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    [Header("발사 쿨다운")]
    public float FireRate = 0.1f;

    public float FireTimer = 0f;

    [Header("투사체")]
    public GameObject BulletPrefab;

    public float BulletSpeed = 10f;
    public float BulletLifeTime = 3f;

    [Header("피해량")]
    public float Damage = 10f;

    public float DamageBonusRate = 0f; // 증강으로 늘어난 공격력 비율

    [Header("공격 속도")]
    public bool IsAttackSpeedBuff;

    public float AttackSpeedBuffRate = 1.5f;
    public bool IsAttackSpeedDebuff;
    public float AttackSpeedDebuffRate = 0.7f;

    public float AttackSpeedBonus = 1f; // 증강으로 늘어난 공격 속도 배율

    [Header("관통")]
    public bool IsPierceBuff;

    public int PierceCount = 3; // 최대 몇 마리까지 관통

    [Header("치명타")]
    public bool IsCritBuff;

    [Range(0, 1)]
    public float CritChance = 0.2f;

    public float CritMultiplier = 2f;

    private Player player;
    private float baseDamage;

    private void Awake()
    {
        player = GetComponent<Player>();
        baseDamage = Damage;
    }

    public void Update()
    {
        // 기절 중에는 발사 불가
        if (player != null && player.IsStunned)
        {
            return;
        }

        bool attackSlowed = IsAttackSpeedDebuff || (player != null && player.IsAttackSlowed);
        float attackSpeed = (IsAttackSpeedBuff ? AttackSpeedBuffRate : 1) *
                            (attackSlowed ? AttackSpeedDebuffRate : 1) *
                            AttackSpeedBonus;
        FireTimer += Time.deltaTime * attackSpeed;
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
            if (Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                           Mathf.Pow(monster.transform.position.z - transform.position.z, 2)) < minDistance)
            {
                minDistance = Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                                         Mathf.Pow(monster.transform.position.z - transform.position.z, 2));
                targetPos = monster.transform.position;
            }
        }

        foreach (var monster in dashMonsters)
        {
            if (Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                           Mathf.Pow(monster.transform.position.z - transform.position.z, 2)) < minDistance)
            {
                minDistance = Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                                         Mathf.Pow(monster.transform.position.z - transform.position.z, 2));
                targetPos = monster.transform.position;
            }
        }

        foreach (var monster in debuffMonsters)
        {
            if (Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                           Mathf.Pow(monster.transform.position.z - transform.position.z, 2)) < minDistance)
            {
                minDistance = Mathf.Sqrt(Mathf.Pow(monster.transform.position.x - transform.position.x, 2) +
                                         Mathf.Pow(monster.transform.position.z - transform.position.z, 2));
                targetPos = monster.transform.position;
            }
        }

        Vector3 targetDirection = Vector3.zero;
        targetDirection = minDistance == float.MaxValue ? transform.forward : targetPos - transform.position;
        targetDirection.y = 0;
        targetDirection.Normalize();

        float critTrial = Random.Range(0f, 1f);

        GameObject gameObject = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        gameObject.GetComponent<PlayerBullet>().MoveSpeed = BulletSpeed;
        gameObject.GetComponent<PlayerBullet>().Direction = targetDirection;
        gameObject.GetComponent<PlayerBullet>().LifeTime = BulletLifeTime;
        gameObject.GetComponent<PlayerBullet>().Damage =
            Damage * (IsCritBuff ? (CritChance >= critTrial ? CritMultiplier : 1) : 1);
        gameObject.GetComponent<PlayerBullet>().RemainPierce = IsPierceBuff ? PierceCount : 1;
    }

    // 증강: 공격 속도 +rate
    public void UpgradeAttackSpeed(float rate)
    {
        AttackSpeedBonus += rate;
    }

    // 증강: 공격력 +rate (기본값 대비 누적)
    public void UpgradeDamage(float rate)
    {
        DamageBonusRate += rate;
        Damage = baseDamage * (1f + DamageBonusRate);
    }

    public void ResetStats()
    {
        Damage = baseDamage;
        DamageBonusRate = 0f;
        AttackSpeedBonus = 1f;
        FireTimer = 0f;
    }
}