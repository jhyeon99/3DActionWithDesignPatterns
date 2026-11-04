using System.Collections.Generic;
using UnityEngine;

public class DamageChainSandbox : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] private float _maxHp = 100f;
    [SerializeField] private float _defensePercent = 15f;

    [Header("공격")]
    [SerializeField] private float _damage = 20f;
    [SerializeField] private bool _isCritical = false;
    [SerializeField] private float _shield = 30f;

    private DamageReceiver _target;
    private int _nextSourceId = 1;

    private void Awake() => CreateTarget();

    private void CreateTarget()
    {
        _target = new DamageReceiver("Fake몬스터", _maxHp, () => _defensePercent);
        _target.OnLogged += Print;
    }

    private static void Print(IReadOnlyList<string> log) => Debug.Log(string.Join("\n", log));

    [ContextMenu("1. 피격")]
    private void Hit() => Run(new HitRequest(_nextSourceId++, _damage, _isCritical));

    [ContextMenu("2. 같은 총알로 두 번")]
    private void DuplicateHit()
    {
        int id = _nextSourceId++;
        Run(new HitRequest(id, _damage, _isCritical));
        Run(new HitRequest(id, _damage, _isCritical));
    }

    [ContextMenu("3. 보호막 추가 후 피격")]
    private void ShieldHit()
    {
        if (_target == null) CreateTarget();
        _target.AddShield(_shield);
        Run(new HitRequest(_nextSourceId++, _damage, _isCritical));
    }

    [ContextMenu("4. 처음부터 다시 (Inspector 값 반영)")]
    private void ResetTarget()
    {
        CreateTarget();
        Debug.Log($"초기화: HP {_target.Hp}, 방어 {_defensePercent}%");
    }

    private void Run(HitRequest hit)
    {
        if (_target == null) CreateTarget();
        DamageResult result = _target.TakeHit(hit);
        Debug.Log($"결과: 깎인 HP {result.AppliedDamage:0.#}, 남은 HP {result.RemainingHp:0.#}, 사망 {result.IsDead}, 중단 {result.StopReason ?? "없음"}");
    }
}
