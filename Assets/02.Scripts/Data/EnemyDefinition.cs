using UnityEngine;

// 적 종류별 수치를 담는 데이터 원본(Type Object).
// 적 종류를 클래스가 아니라 에셋 파일로 구분하므로, 새 적은 에셋만 만들면 되고 수치는 인스펙터에서 바꾼다.
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public string Id = "Enemy";
    public float MaxHealth = 100f;
    public float MoveSpeed = 1.5f;
    public float AttackDamage = 20f;
    public float AttackInterval = 0.5f;
    public float ScoreValue = 10f;

    // 인스펙터에서 값을 바꿀 때마다 잘못된 값을 바로잡는다.
    private void OnValidate()
    {
        MaxHealth = Mathf.Max(1f, MaxHealth);
        MoveSpeed = Mathf.Max(0f, MoveSpeed);
        AttackDamage = Mathf.Max(0f, AttackDamage);
        AttackInterval = Mathf.Max(0.01f, AttackInterval);
        ScoreValue = Mathf.Max(0f, ScoreValue);
    }

    // 게임 중에는 원본을 직접 쓰지 않고, 이 복사본(Spec)을 읽는다.
    public EnemySpec ToSpec()
    {
        return new EnemySpec(Id, MaxHealth, MoveSpeed, AttackDamage, AttackInterval, ScoreValue);
    }
}
