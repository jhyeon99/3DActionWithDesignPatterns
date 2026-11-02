using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerStat))]
public class PlayerFire : MonoBehaviour
{
    [Header("투사체")]
    [FormerlySerializedAs("BulletPrefab")]
    [SerializeField] private GameObject _bulletPrefab;

    [Header("공격 속도")]
    [FormerlySerializedAs("IsAttackSpeedBuff")]
    [SerializeField] private bool _isAttackSpeedBuff;
    [FormerlySerializedAs("IsAttackSpeedDebuff")]
    [SerializeField] private bool _isAttackSpeedDebuff;

    [Header("관통")]
    [FormerlySerializedAs("IsPierceBuff")]
    [SerializeField] private bool _isPierceBuff;

    [Header("치명타")]
    [FormerlySerializedAs("IsCritBuff")]
    [SerializeField] private bool _isCritBuff;

    private float _fireTimer = 0f; // 발사 쿨다운 누적 시간

    private Player _player;
    private PlayerStat _stat;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _stat = GetComponent<PlayerStat>();
    }

    private void Update()
    {
        // 기절 중에는 발사 불가
        if (_player != null && _player.IsStunned)
        {
            return;
        }

        bool attackSlowed = _isAttackSpeedDebuff || (_player != null && _player.IsAttackSlowed);
        float attackSpeed = (_isAttackSpeedBuff ? _stat.AttackSpeedBuffRate : 1) *
                            (attackSlowed ? _stat.AttackSpeedDebuffRate : 1) *
                            _stat.AttackSpeedBonus;
        _fireTimer += Time.deltaTime * attackSpeed;
        if (_fireTimer >= _stat.FireRate)
        {
            Fire();
            _fireTimer -= _stat.FireRate;
        }
    }

    private void Fire()
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

        float damage = _stat.Damage * (_isCritBuff ? (_stat.CritChance >= critTrial ? _stat.CritMultiplier : 1) : 1);
        int remainPierce = _isPierceBuff ? _stat.PierceCount : 1;

        GameObject bulletObject = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        bulletObject.GetComponent<PlayerBullet>().Init(_stat.BulletSpeed, targetDirection, _stat.BulletLifeTime, damage, remainPierce);
    }

    // 증강: 공격 속도 +rate
    public void UpgradeAttackSpeed(float rate)
    {
        _stat.UpgradeAttackSpeed(rate);
    }

    // 증강: 공격력 +rate (기본값 대비 누적)
    public void UpgradeDamage(float rate)
    {
        _stat.UpgradeDamage(rate);
    }

    public void ResetStats()
    {
        _stat.ResetAttackBonus();
        _fireTimer = 0f;
    }
}