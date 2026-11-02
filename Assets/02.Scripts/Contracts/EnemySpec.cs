// 적 하나의 수치 묶음. EnemyDefinition(원본)에서 복사해 만든 읽기 전용 데이터라서,
// 게임 중에 이 값을 바꿔도 ScriptableObject 원본은 바뀌지 않는다.
public readonly struct EnemySpec
{
    public readonly string Id;
    public readonly float MaxHealth;
    public readonly float MoveSpeed;
    public readonly float AttackDamage;
    public readonly float AttackInterval;
    public readonly float ScoreValue;

    public EnemySpec(string id, float maxHealth, float moveSpeed, float attackDamage, float attackInterval, float scoreValue)
    {
        Id = id;
        MaxHealth = maxHealth;
        MoveSpeed = moveSpeed;
        AttackDamage = attackDamage;
        AttackInterval = attackInterval;
        ScoreValue = scoreValue;
    }
}
