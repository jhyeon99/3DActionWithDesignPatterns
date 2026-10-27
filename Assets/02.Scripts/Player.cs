using UnityEngine;

public enum PlayerDebuff
{
    MoveSlow,
    AttackSlow,
    Stun
}

public class Player : MonoBehaviour
{
    [Header("HP")]
    public float MaxHp = 100f;
    public float DefensePercent = 15f;

    [Header("Debuff")]
    [Tooltip("이동 속도 감소 디버프 중 적용되는 배율")]
    public float MoveSlowRate = 0.6f;
    public float MoveSlowTimer = 0f;
    public float AttackSlowTimer = 0f;
    public float StunTimer = 0f;

    [Header("Buff")]
    public float HealAmount = 20f;

    [Header("Key")]
    [Tooltip("임시로 만든 키")]
    public KeyCode HealKey = KeyCode.Alpha1;
    public KeyCode DamageKey = KeyCode.Alpha2;

    [Header("Test Damage")]
    public float TestDamage = 10f;

    // [Header("Hit")]
    // public float HitStunTime = 0.1f;

    public float CurrentHp = 100f;
    // public float HitStunTimer = 0f;
    public bool IsDead = false;
    // public bool IsHitStun = false;
    public string HpText = "";

    public bool IsStunned => StunTimer > 0f;
    public bool IsMoveSlowed => MoveSlowTimer > 0f;
    public bool IsAttackSlowed => AttackSlowTimer > 0f;

    // 새 런 시작 시 복원할 초기값
    private float initialMaxHp;
    private float initialDefense;
    private Vector3 startPosition;

    public void Awake()
    {
        initialMaxHp = MaxHp;
        initialDefense = DefensePercent;
        startPosition = transform.position;
    }

    public void Start()
    {
        CurrentHp = MaxHp;
    }

    public void Update()
    {
        if (IsDead)
        {
            return;
        }

        StunTimer = Mathf.Max(0f, StunTimer - Time.deltaTime);
        MoveSlowTimer = Mathf.Max(0f, MoveSlowTimer - Time.deltaTime);
        AttackSlowTimer = Mathf.Max(0f, AttackSlowTimer - Time.deltaTime);

        // 임시 버프 키
        if (Input.GetKeyDown(HealKey))
        {
            UpgradeHeal(HealAmount);
        }

        // 임시 대미지 키
        if (Input.GetKeyDown(DamageKey))
        {
            TakeDamage(TestDamage);
        }

        // if (HitStunTimer > 0f)
        // {
        //     HitStunTimer -= Time.deltaTime;
        // }

        // IsHitStun = HitStunTimer > 0f;

        HpText = $"HP {CurrentHp:0.#} / {MaxHp:0.#}";
    }

    public void TakeDamage(float damage)
    {
        if (IsDead)
        {
            return;
        }

        float defense = DefensePercent;
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
        CurrentHp -= damage;
        DebugStats.Log($"Player hit {rawDamage:0.#} -> def {defense:0.#}% -> {damage:0.#} (HP {Mathf.Max(0f, CurrentHp):0.#})");
        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            IsDead = true;
            HpText = $"HP 0 / {MaxHp:0.#}";
            DebugStats.Log("Player dead");
        }

        // HitStunTimer = HitStunTime;
    }

    public void UpgradeMaxHp(float amount)
    {
        if (IsDead)
        {
            return;
        }

        MaxHp += amount;
        CurrentHp += amount;
    }

    public void UpgradeDefense(float amount)
    {
        if (IsDead)
        {
            return;
        }

        DefensePercent += amount;
    }

    public void UpgradeHeal(float amount)
    {
        if (IsDead)
        {
            return;
        }

        CurrentHp += amount;
        if (CurrentHp > MaxHp)
        {
            CurrentHp = MaxHp;
        }
    }

    // 몬스터가 거는 디버프. 같은 종류는 더 긴 지속 시간으로 갱신한다.
    public void ApplyDebuff(PlayerDebuff debuff, float duration)
    {
        if (IsDead)
        {
            return;
        }

        switch (debuff)
        {
            case PlayerDebuff.MoveSlow:
                MoveSlowTimer = Mathf.Max(MoveSlowTimer, duration);
                break;
            case PlayerDebuff.AttackSlow:
                AttackSlowTimer = Mathf.Max(AttackSlowTimer, duration);
                break;
            case PlayerDebuff.Stun:
                StunTimer = Mathf.Max(StunTimer, duration);
                break;
        }

        DebugStats.Log($"Player debuff {debuff} {duration:0.#}s");
    }

    // 새 런 시작: 강화/체력/디버프/위치 초기화
    public void ResetForNewRun()
    {
        MaxHp = initialMaxHp;
        DefensePercent = initialDefense;
        CurrentHp = MaxHp;
        IsDead = false;
        StunTimer = 0f;
        MoveSlowTimer = 0f;
        AttackSlowTimer = 0f;
        HpText = $"HP {CurrentHp:0.#} / {MaxHp:0.#}";

        // CharacterController가 켜져 있으면 transform.position 대입이 무시된다.
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        transform.position = startPosition;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    public void OnGUI()
    {
        float hpRatio = CurrentHp / MaxHp;
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
                GUI.Label(outlineRect, HpText, hpTextStyle);
            }
        }

        hpTextStyle.normal.textColor = Color.white;
        GUI.Label(hpTextRect, HpText, hpTextStyle);
    }
}