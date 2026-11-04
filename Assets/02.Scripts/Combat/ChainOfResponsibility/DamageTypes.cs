public interface IDamageable
{
    DamageResult TakeHit(HitRequest hit);
}

public readonly struct HitRequest
{
    public int SourceId { get; }
    public float Damage { get; }
    public bool IsCritical { get; }

    public HitRequest(int sourceId, float damage, bool isCritical = false)
    {
        SourceId = sourceId;
        Damage = damage;
        IsCritical = isCritical;
    }
}

public readonly struct DamageResult
{
    public float AppliedDamage { get; }
    public float RemainingHp { get; }
    public bool IsDead { get; }
    public string StopReason { get; }

    public DamageResult(float appliedDamage, float remainingHp, bool isDead, string stopReason)
    {
        AppliedDamage = appliedDamage;
        RemainingHp = remainingHp;
        IsDead = isDead;
        StopReason = stopReason;
    }
}
