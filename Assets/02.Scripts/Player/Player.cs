using UnityEngine;
using UnityEngine.Serialization;

public enum PlayerDebuff
{
    MoveSlow,
    AttackSlow,
    Stun
}

[RequireComponent(typeof(PlayerStat))]
public class Player : MonoBehaviour
{
    [Header("Buff")]
    [FormerlySerializedAs("HealAmount")]
    [SerializeField] private float _healAmount = 20f;

    [Header("Key")]
    [Tooltip("임시로 만든 키")]
    [FormerlySerializedAs("HealKey")]
    [SerializeField] private KeyCode _healKey = KeyCode.Alpha1;
    [FormerlySerializedAs("DamageKey")]
    [SerializeField] private KeyCode _damageKey = KeyCode.Alpha2;

    [Header("Test Damage")]
    [FormerlySerializedAs("TestDamage")]
    [SerializeField] private float _testDamage = 10f;




    // [Header("Hit")]
    // public float HitStunTime = 0.1f;

    private float _moveSlowTimer = 0f;
    private float _attackSlowTimer = 0f;
    private float _stunTimer = 0f;

    private float _currentHp = 100f;
    // public float HitStunTimer = 0f;
    private bool _isDead = false;
    // public bool IsHitStun = false;
    private string _hpText = "";

    public bool IsStunned => _stunTimer > 0f;
    public bool IsMoveSlowed => _moveSlowTimer > 0f;
    public bool IsAttackSlowed => _attackSlowTimer > 0f;

    private PlayerStat _stat;

    // 새 런 시작 시 복원할 위치
    private Vector3 _startPosition;

    private void Awake()
    {
        _stat = GetComponent<PlayerStat>();
        _startPosition = transform.position;
    }

    private void Start()
    {
        _currentHp = _stat.MaxHp;
    }

    private void Update()
    {
        if (_isDead)
        {
            return;
        }

        _stunTimer = Mathf.Max(0f, _stunTimer - Time.deltaTime);
        _moveSlowTimer = Mathf.Max(0f, _moveSlowTimer - Time.deltaTime);
        _attackSlowTimer = Mathf.Max(0f, _attackSlowTimer - Time.deltaTime);

        // 임시 버프 키
        if (Input.GetKeyDown(_healKey))
        {
            UpgradeHeal(_healAmount);
        }

        // 임시 대미지 키
        if (Input.GetKeyDown(_damageKey))
        {
            TakeDamage(_testDamage);
        }

        // if (HitStunTimer > 0f)
        // {
        //     HitStunTimer -= Time.deltaTime;
        // }

        // IsHitStun = HitStunTimer > 0f;

        _hpText = $"HP {_currentHp:0.#} / {_stat.MaxHp:0.#}";
    }

    public void TakeDamage(float damage)
    {
        if (_isDead)
        {
            return;
        }

        float defense = _stat.DefensePercent;
        float rawDamage = damage;

        if (defense > 100f)
        {
            defense = 100f;
        }

        damage = damage * (1f - defense / 100f);

        if (damage < 0f)
        {
            damage = 0f;
        }
        _currentHp -= damage;
        DebugStats.Log($"Player hit {rawDamage:0.#} -> def {defense:0.#}% -> {damage:0.#} (HP {Mathf.Max(0f, _currentHp):0.#})");
        if (_currentHp <= 0f)
        {
            _currentHp = 0f;
            _isDead = true;
            _hpText = $"HP 0 / {_stat.MaxHp:0.#}";
            DebugStats.Log("Player dead");
        }

        // HitStunTimer = HitStunTime;
    }

    public void UpgradeMaxHp(float amount)
    {
        if (_isDead)
        {
            return;
        }

        _stat.UpgradeMaxHp(amount);
        _currentHp += amount;
    }

    public void UpgradeDefense(float amount)
    {
        if (_isDead)
        {
            return;
        }

        _stat.UpgradeDefense(amount);
    }

    public void UpgradeHeal(float amount)
    {
        if (_isDead)
        {
            return;
        }

        _currentHp += amount;
        if (_currentHp > _stat.MaxHp)
        {
            _currentHp = _stat.MaxHp;
        }
    }

    // 몬스터가 거는 디버프. 같은 종류는 더 긴 지속 시간으로 갱신한다.
    public void ApplyDebuff(PlayerDebuff debuff, float duration)
    {
        if (_isDead)
        {
            return;
        }

        switch (debuff)
        {
            case PlayerDebuff.MoveSlow:
                _moveSlowTimer = Mathf.Max(_moveSlowTimer, duration);
                break;
            case PlayerDebuff.AttackSlow:
                _attackSlowTimer = Mathf.Max(_attackSlowTimer, duration);
                break;
            case PlayerDebuff.Stun:
                _stunTimer = Mathf.Max(_stunTimer, duration);
                break;
        }

        DebugStats.Log($"Player debuff {debuff} {duration:0.#}s");
    }

    // 새 런 시작: 강화/체력/디버프/위치 초기화
    public void ResetForNewRun()
    {
        _stat.ResetHpBonus();
        _currentHp = _stat.MaxHp;
        _isDead = false;
        _stunTimer = 0f;
        _moveSlowTimer = 0f;
        _attackSlowTimer = 0f;
        _hpText = $"HP {_currentHp:0.#} / {_stat.MaxHp:0.#}";

        // CharacterController가 켜져 있으면 transform.position 대입이 무시된다.
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = _startPosition;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private void OnGUI()
    {
        float hpRatio = _currentHp / _stat.MaxHp;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(20f, 20f, 300f, 30f), Texture2D.whiteTexture);
        GUI.color = Color.Lerp(Color.red, Color.green, hpRatio);
        GUI.DrawTexture(new Rect(20f, 20f, 300f * hpRatio, 30f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle hpTextStyle = new(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };
        Rect hpTextRect = new(28f, 20f, 300f, 30f);

        float outline = 1.5f;
        hpTextStyle.normal.textColor = Color.black;
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                Rect outlineRect = new(hpTextRect.x + x * outline, hpTextRect.y + y * outline, hpTextRect.width, hpTextRect.height);
                GUI.Label(outlineRect, _hpText, hpTextStyle);
            }
        }

        hpTextStyle.normal.textColor = Color.white;
        GUI.Label(hpTextRect, _hpText, hpTextStyle);
    }
}