using UnityEngine;

// 적 생성 계약. 김민석(Factory/Pool)이 구현한다.
// GameManager 연결은 다음 단계에서 한다. (지금은 계약만 정의)
public interface IEnemySpawner
{
    // monsterIndex 종류의 적을 pointIndex 스폰 포인트에 만든다. 만들지 못하면 null을 돌려준다.
    GameObject Spawn(int monsterIndex, int pointIndex);

    // 살아 있는 적을 모두 제거한다.
    void ClearAll();
}
