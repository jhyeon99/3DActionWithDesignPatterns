using UnityEngine;

// 웨이브가 언제 시작되고 몇 마리를 소환할지 정한다. MonoBehaviour가 아닌 순수 C# 클래스다.
// 몬스터를 실제로 만드는 일은 하지 않는다. (소환은 GameManager, 나중에는 IEnemySpawner가 맡는다)
public sealed class WaveDirector
{
    // 웨이브가 한 번 시작될 때마다 늘어나는 몬스터 수
    private const int IncreasePerStep = 5;

    private float timer;

    // 타이머를 가득 채워서, 다음 Tick에 바로 첫 웨이브가 시작되게 한다.
    public void StartNow(float interval)
    {
        timer = interval;
    }

    // 웨이브를 시작했으니 타이머를 처음으로 되돌린다.
    public void ResetTimer()
    {
        timer = 0f;
    }

    // 매 프레임 호출한다. 웨이브를 시작할 때가 되면 true를 돌려준다.
    public bool Tick(float deltaTime, float interval)
    {
        timer += deltaTime;
        return timer >= interval;
    }

    // 지난 시간이 길수록 소환할 몬스터 수가 늘어난다.
    public int GetSpawnCount(float elapsedTime, int initialCount, float increaseInterval)
    {
        int steps = Mathf.FloorToInt(elapsedTime / Mathf.Max(1f, increaseInterval));
        return initialCount + steps * IncreasePerStep;
    }
}
