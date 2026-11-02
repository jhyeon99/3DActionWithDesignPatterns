using UnityEngine;

// 게임에서 쓰는 서비스를 한곳에서 등록하는 시작점. 씬에 하나만 둔다.
// 다른 스크립트의 Awake보다 먼저 실행되도록 실행 순서를 가장 앞으로 당긴다.
[DefaultExecutionOrder(-1000)]
public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        RegisterDefaultServices();
    }

    private void OnDestroy()
    {
        ServiceLocator.Clear();
    }

    // 아직 등록되지 않은 서비스만 기본(Legacy) 구현으로 등록한다.
    // 다른 담당이 자기 구현을 먼저 등록했다면 그대로 두고, 이 씬에 GameBootstrap이 없으면 GameManager가 대신 부른다.
    // 서비스를 새로 만들 때 여기에 한 줄씩 추가한다.
    public static void RegisterDefaultServices()
    {
        if (!ServiceLocator.TryGet(out IClock _))
        {
            ServiceLocator.Register<IClock>(new UnityClock());
        }

        if (!ServiceLocator.TryGet(out IGameEventBus _))
        {
            ServiceLocator.Register<IGameEventBus>(new GameEventBus());
        }

        if (!ServiceLocator.TryGet(out IPlayerStatus _))
        {
            ServiceLocator.Register<IPlayerStatus>(new LegacyPlayerStatus());
        }

        if (!ServiceLocator.TryGet(out IUpgradeApplier _))
        {
            ServiceLocator.Register<IUpgradeApplier>(new LegacyUpgradeApplier());
        }
    }
}
