using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("HP")]
    public float MaxHp = 100f;
    public float DefensePercent = 15f;

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
        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            IsDead = true;
            HpText = $"HP 0 / {MaxHp:0.#}";
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