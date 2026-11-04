using System.Collections.Generic;
using UnityEngine;

public partial class DamageReceiver
{
    private abstract class DamageHandler
    {
        private DamageHandler _next;

        public DamageHandler SetNext(DamageHandler next)
        {
            _next = next;
            return next;
        }

        public void Handle(DamageContext ctx)
        {
            Process(ctx);
            if (!ctx.Stopped && _next != null)
            {
                _next.Handle(ctx);
            }
        }

        protected abstract void Process(DamageContext ctx);
    }

    private class DuplicateHitHandler : DamageHandler
    {
        private readonly HashSet<int> _hitSources = new HashSet<int>();

        public void Clear() => _hitSources.Clear();

        protected override void Process(DamageContext ctx)
        {
            int id = ctx.Hit.SourceId;
            if (id == 0)
            {
                ctx.Log.Add("중복타격: 검사 안 함");
                return;
            }
            if (!_hitSources.Add(id))
            {
                ctx.Stop($"이미 맞은 공격 #{id}");
                return;
            }
            ctx.Log.Add($"중복타격: 통과 (#{id})");
        }
    }

    private class AttackHandler : DamageHandler
    {
        protected override void Process(DamageContext ctx)
        {
            ctx.Damage = ctx.Hit.Damage;
            ctx.Log.Add($"공격: {ctx.Damage:0.#}{(ctx.Hit.IsCritical ? " (치명타)" : "")}");
            if (ctx.Damage <= 0f)
            {
                ctx.Stop("피해 0");
            }
        }
    }

    private class DefenseHandler : DamageHandler
    {
        protected override void Process(DamageContext ctx)
        {
            float defense = Mathf.Clamp(ctx.DefensePercent, 0f, 100f);
            ctx.Damage *= 1f - defense / 100f;
            ctx.Log.Add($"방어: {defense:0.#}% → {ctx.Damage:0.#}");
            if (ctx.Damage <= 0f)
            {
                ctx.Stop("방어로 전부 막음");
            }
        }
    }

    private class ShieldHandler : DamageHandler
    {
        protected override void Process(DamageContext ctx)
        {
            float absorbed = Mathf.Min(ctx.Health.Shield, ctx.Damage);
            if (absorbed <= 0f)
            {
                ctx.Log.Add("보호막: 없음");
                return;
            }
            ctx.Health.ReduceShield(absorbed);
            ctx.Damage -= absorbed;
            ctx.Log.Add($"보호막: {absorbed:0.#} 흡수 → {ctx.Damage:0.#} (남은 보호막 {ctx.Health.Shield:0.#})");
            if (ctx.Damage <= 0f)
            {
                ctx.Stop("보호막이 전부 막음");
            }
        }
    }

    private class HpHandler : DamageHandler
    {
        protected override void Process(DamageContext ctx)
        {
            float before = ctx.Health.Hp;
            ctx.Health.Reduce(ctx.Damage);
            ctx.AppliedDamage = before - ctx.Health.Hp;
            ctx.Log.Add($"HP: {before:0.#} → {ctx.Health.Hp:0.#}{(ctx.Health.IsDead ? " (사망)" : "")}");
        }
    }
}
