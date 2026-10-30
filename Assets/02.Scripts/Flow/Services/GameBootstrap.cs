using UnityEngine;

// 게임에서 쓰는 서비스를 한곳에서 등록하는 시작점. 씬에 하나만 둔다.
// 다른 스크립트의 Awake보다 먼저 실행되도록 실행 순서를 가장 앞으로 당긴다.
[DefaultExecutionOrder(-1000)]
public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        // 서비스를 새로 만들 때 여기에 한 줄씩 추가한다.
        // TODO: 이벤트 버스(IGameEventBus)는 feature/E-Observer와 합친 뒤 등록한다.
        ServiceLocator.Register<IClock>(new UnityClock());
    }

    private void OnDestroy()
    {
        ServiceLocator.Clear();
    }
}
