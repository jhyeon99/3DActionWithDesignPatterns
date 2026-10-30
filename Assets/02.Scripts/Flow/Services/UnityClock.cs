using UnityEngine;

// IClock의 기본 구현. Unity의 시간을 그대로 돌려준다.
public sealed class UnityClock : IClock
{
    public float Time => UnityEngine.Time.time;

    public float DeltaTime => UnityEngine.Time.deltaTime;
}
