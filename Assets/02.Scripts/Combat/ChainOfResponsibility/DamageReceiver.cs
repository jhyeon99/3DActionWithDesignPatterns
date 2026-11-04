using System;
using System.Collections.Generic;

public partial class DamageReceiver : IDamageable
{
    private readonly string _name;
    private readonly Func<float> _getDefense;
    private readonly HealthStore _health;
    private readonly DuplicateHitHandler _duplicate;
    private readonly DamageHandler _head;

    public event Action<IReadOnlyList<string>> OnLogged;

    public float Hp => _health.Hp;
    public float MaxHp => _health.MaxHp;
    public float Shield => _health.Shield;
    public bool IsDead => _health.IsDead;

    public DamageReceiver(string name, float maxHp, Func<float> getDefense)
    {
        _name = name;
        _getDefense = getDefense ?? (() => 0f);
        _health = new HealthStore(maxHp);

        _duplicate = new DuplicateHitHandler();
        _head = _duplicate;
        _duplicate
            .SetNext(new AttackHandler())
            .SetNext(new DefenseHandler())
            .SetNext(new ShieldHandler())
            .SetNext(new HpHandler());
    }

    public DamageResult TakeHit(HitRequest hit)
    {
        if (_health.IsDead)
        {
            return new DamageResult(0f, 0f, true, "이미 사망");
        }

        var ctx = new DamageContext(hit, _health, _getDefense());
        ctx.Log.Add($"[Hit] #{hit.SourceId} → {_name}");
        _head.Handle(ctx);
        OnLogged?.Invoke(ctx.Log);
        return ctx.ToResult();
    }

    public void Heal(float amount) => _health.Heal(amount);
    public void AddShield(float amount) => _health.AddShield(amount);
    public void SetMaxHp(float maxHp, bool refill) => _health.SetMaxHp(maxHp, refill);

    public void ResetForReuse()
    {
        _health.Reset();
        _duplicate.Clear();
    }

    private class DamageContext
    {
        public HitRequest Hit { get; }
        public HealthStore Health { get; }
        public float DefensePercent { get; }
        public List<string> Log { get; } = new List<string>();
        public float Damage { get; set; }
        public float AppliedDamage { get; set; }
        public bool Stopped { get; private set; }
        public string StopReason { get; private set; }

        public DamageContext(HitRequest hit, HealthStore health, float defensePercent)
        {
            Hit = hit;
            Health = health;
            DefensePercent = defensePercent;
        }

        public void Stop(string reason)
        {
            Stopped = true;
            StopReason = reason;
            Log.Add($"  → 중단: {reason}");
        }

        public DamageResult ToResult() => new DamageResult(AppliedDamage, Health.Hp, Health.IsDead, StopReason);
    }
}
