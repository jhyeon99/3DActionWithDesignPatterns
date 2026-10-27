using System;
using UnityEngine;

public enum StatKind
{
    Bullet,
    NormalMonster,
    DashMonster,
    DebuffMonster,
    Exp
}

/// <summary>
/// 리팩토링 전/후 효율 비교용 계측.
/// 객체의 Awake / OnDestroy 에서 호출해 생성·파괴 횟수를 센다.
/// (풀링 적용 후에는 OnSpawn / OnDespawn 에서 호출하면 같은 지표로 비교할 수 있다.)
/// </summary>
public static class DebugStats
{
    private const int KindCount = 5;
    private const int LogCapacity = 8;

    public static readonly int[] Created = new int[KindCount];
    public static readonly int[] Destroyed = new int[KindCount];
    public static int LogCount;

    private static readonly string[] logs = new string[LogCapacity];
    private static int logHead;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        Array.Clear(Created, 0, KindCount);
        Array.Clear(Destroyed, 0, KindCount);
        Array.Clear(logs, 0, LogCapacity);
        logHead = 0;
        LogCount = 0;
    }

    public static int Active(StatKind kind)
    {
        return Created[(int)kind] - Destroyed[(int)kind];
    }

    public static void OnCreated(StatKind kind)
    {
        Created[(int)kind]++;
    }

    public static void OnDestroyed(StatKind kind)
    {
        Destroyed[(int)kind]++;
    }

    // 전투 로그 (최근 LogCapacity개만 유지)
    public static void Log(string message)
    {
        logs[logHead] = $"[{Time.time:0.0}] {message}";
        logHead = (logHead + 1) % LogCapacity;
        if (LogCount < LogCapacity)
        {
            LogCount++;
        }
    }

    // index 0 = 가장 최근 로그
    public static string GetLog(int index)
    {
        int i = (logHead - 1 - index + LogCapacity * 2) % LogCapacity;
        return logs[i];
    }
}
