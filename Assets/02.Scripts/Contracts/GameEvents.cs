using UnityEngine;

// 게임 이벤트 데이터. 모두 불변 struct라서 발행할 때 가비지가 생기지 않는다.
// 이름 끝의 Event는 GameManager.GameOver()/LevelUp() 같은 기존 메서드 이름과 겹치지 않게 하려는 것이다.
// 주석의 "발행"은 이 이벤트를 발행할 담당이다. (2~3주차 작업 분담 계획 기준)

// 발행: 송재현(Flow)
public readonly struct GameStartedEvent
{
}

// 발행: 송재현(Flow)
public readonly struct GameOverEvent
{
    public readonly float SurvivedTime;
    public readonly float Score;
    public readonly int KillCount;

    public GameOverEvent(float survivedTime, float score, int killCount)
    {
        SurvivedTime = survivedTime;
        Score = score;
        KillCount = killCount;
    }
}

// 발행: 송재현(WaveDirector)
public readonly struct WaveStartedEvent
{
    public readonly int SpawnCount;
    public readonly float ElapsedTime;

    public WaveStartedEvent(int spawnCount, float elapsedTime)
    {
        SpawnCount = spawnCount;
        ElapsedTime = elapsedTime;
    }
}

// 발행: 장준혁(Monster). 적이 죽는 순간 한 번. (테스트 코드)
// EnemyId가 Contracts에 생기면 MonsterName을 EnemyId로 바꾼다.
public readonly struct EnemyKilledEvent
{
    public readonly string MonsterName;
    public readonly float ScoreValue;
    public readonly Vector3 Position;

    public EnemyKilledEvent(string monsterName, float scoreValue, Vector3 position)
    {
        MonsterName = monsterName;
        ScoreValue = scoreValue;
        Position = position;
    }
}

// 발행: 김민석(Exp). 경험치 구슬을 먹는 순간 한 번. (테스트 코드)
public readonly struct ExpCollectedEvent
{
    public readonly float Amount;

    public ExpCollectedEvent(float amount)
    {
        Amount = amount;
    }
}

// 발행: 송재현(Flow). 경험치가 차서 레벨이 오를 때.
public readonly struct LevelUpEvent
{
    public readonly int Level;

    public LevelUpEvent(int level)
    {
        Level = level;
    }
}

// 발행: 송재현(ScoreService)
public readonly struct ScoreChangedEvent
{
    public readonly float Score;

    public ScoreChangedEvent(float score)
    {
        Score = score;
    }
}

// 발행: 정재윤(PlayerHealth). HP가 바뀔 때마다. (테스트 코드)
public readonly struct PlayerHealthChangedEvent
{
    public readonly float CurrentHp;
    public readonly float MaxHp;

    public PlayerHealthChangedEvent(float currentHp, float maxHp)
    {
        CurrentHp = currentHp;
        MaxHp = maxHp;
    }
}

// 발행: 정재윤(PlayerHealth). 플레이어가 죽는 순간 한 번. (테스트 코드)
public readonly struct PlayerDiedEvent
{
}