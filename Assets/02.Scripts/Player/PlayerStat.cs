using UnityEngine;

// 역할 : 플레이어의 체력·공격·이동 스탯을 한 곳에서 관리한다.
// Inspector 값은 기본값으로 두고, 증강으로 늘어난 양은 따로 누적해서 새 런 시작 시 되돌린다.
public class PlayerStat : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _defensePercent = 15f;

    [Header("Debuff")]
    [Tooltip("이동 속도 감소 디버프 중 적용되는 배율")]
    [SerializeField] private float _moveSlowRate = 0.6f;

    [Header("발사 쿨다운")]
    [SerializeField] private float _fireRate = 0.1f;

    [Header("투사체")]
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _bulletLifeTime = 3f;

    [Header("피해량")]
    [SerializeField] private float _damage = 10f;

    [Header("공격 속도")]
    [SerializeField] private float _attackSpeedBuffRate = 1.5f;
    [SerializeField] private float _attackSpeedDebuffRate = 0.7f;

    [Header("관통")]
    [SerializeField] private int _pierceCount = 3; // 최대 몇 마리까지 관통

    [Header("치명타")]
    [Range(0, 1)]
    [SerializeField] private float _critChance = 0.2f;
    [SerializeField] private float _critMultiplier = 2f;

    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dodgeSpeed = 30f;
    [SerializeField] private float _dodgeTime = 0.5f;
    [SerializeField] private float _jumpSpeed = 10f; // 음 이거 왜있음?

    // 증강으로 늘어난 양
    private float _maxHpBonus = 0f;
    private float _defenseBonus = 0f;
    private float _damageBonusRate = 0f; // 기본 공격력 대비 비율
    private float _attackSpeedBonus = 1f; // 공격 속도 배율
    private float _moveSpeedBonusRate = 0f; // 기본 이동 속도 대비 비율

    public float MaxHp => _maxHp + _maxHpBonus;
    public float DefensePercent => _defensePercent + _defenseBonus;
    public float MoveSlowRate => _moveSlowRate;

    public float FireRate => _fireRate;
    public float BulletSpeed => _bulletSpeed;
    public float BulletLifeTime => _bulletLifeTime;
    public float Damage => _damage * (1f + _damageBonusRate);
    public float AttackSpeedBuffRate => _attackSpeedBuffRate;
    public float AttackSpeedDebuffRate => _attackSpeedDebuffRate;
    public float AttackSpeedBonus => _attackSpeedBonus;
    public int PierceCount => _pierceCount;
    public float CritChance => _critChance;
    public float CritMultiplier => _critMultiplier;

    public float MoveSpeed => _moveSpeed * (1f + _moveSpeedBonusRate);
    public float DodgeSpeed => _dodgeSpeed;
    public float DodgeTime => _dodgeTime;
    public float JumpSpeed => _jumpSpeed;

    // 증강: 최대 체력 +amount
    public void UpgradeMaxHp(float amount)
    {
        _maxHpBonus += amount;
    }

    // 증강: 방어력 +amount (%)
    public void UpgradeDefense(float amount)
    {
        _defenseBonus += amount;
    }

    // 증강: 공격력 +rate (기본값 대비 누적)
    public void UpgradeDamage(float rate)
    {
        _damageBonusRate += rate;
    }

    // 증강: 공격 속도 +rate
    public void UpgradeAttackSpeed(float rate)
    {
        _attackSpeedBonus += rate;
    }

    // 증강: 이동 속도 +rate (기본값 대비 누적)
    public void UpgradeMoveSpeed(float rate)
    {
        _moveSpeedBonusRate += rate;
    }

    public void ResetHpBonus()
    {
        _maxHpBonus = 0f;
        _defenseBonus = 0f;
    }

    public void ResetAttackBonus()
    {
        _damageBonusRate = 0f;
        _attackSpeedBonus = 1f;
    }

    public void ResetMoveBonus()
    {
        _moveSpeedBonusRate = 0f;
    }
}
