using UnityEngine;

public partial class DamageReceiver
{
    private class HealthStore
    {
        public float MaxHp { get; private set; }
        public float Hp { get; private set; }
        public float Shield { get; private set; }
        public bool IsDead => Hp <= 0f;

        public HealthStore(float maxHp)
        {
            MaxHp = maxHp;
            Hp = maxHp;
        }

        public void Reduce(float amount) => Hp = Mathf.Max(0f, Hp - amount);
        public void Heal(float amount) => Hp = Mathf.Min(MaxHp, Hp + amount);
        public void AddShield(float amount) => Shield += amount;
        public void ReduceShield(float amount) => Shield = Mathf.Max(0f, Shield - amount);

        public void SetMaxHp(float maxHp, bool refill)
        {
            MaxHp = maxHp;
            Hp = refill ? maxHp : Mathf.Min(Hp, maxHp);
        }

        public void Reset()
        {
            Hp = MaxHp;
            Shield = 0f;
        }
    }
}
