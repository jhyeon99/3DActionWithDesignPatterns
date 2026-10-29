using UnityEngine;

// Test_Observer 씬용 검증 컴포넌트. 빈 오브젝트에 붙이고 플레이한다.
// 이벤트 하나를 발행했을 때 구독자 여러 개(점수, 처치 수, 로그)가 동시에 반응하는지 본다.
public class EventBusTester : MonoBehaviour
{
    [Header("테스트 키")]
    public KeyCode KillKey = KeyCode.F6;
    public KeyCode ExpKey = KeyCode.F7;
    public KeyCode ToggleLogKey = KeyCode.F8;

    [Header("테스트 값")]
    public float KillScore = 10f;
    public float ExpAmount = 5f;

    private IGameEventBus bus;
    private float score;
    private int killCount;
    private float exp;
    private bool isLogSubscribed;

    private void Awake()
    {
        bus = new GameEventBus();
    }

    private void OnEnable()
    {
        bus.Subscribe<EnemyKilledEvent>(OnKilledScore);
        bus.Subscribe<EnemyKilledEvent>(OnKilledCount);
        bus.Subscribe<ExpCollectedEvent>(OnExpCollected);
        SubscribeLog();
    }

    // 구독한 이벤트는 반드시 해제한다.
    private void OnDisable()
    {
        bus.Unsubscribe<EnemyKilledEvent>(OnKilledScore);
        bus.Unsubscribe<EnemyKilledEvent>(OnKilledCount);
        bus.Unsubscribe<ExpCollectedEvent>(OnExpCollected);
        UnsubscribeLog();
    }

    private void Update()
    {
        // 발행하는 쪽은 구독자가 누구인지 모른다.
        if (Input.GetKeyDown(KillKey))
        {
            bus.Publish(new EnemyKilledEvent("TestMonster", KillScore, transform.position));
        }

        if (Input.GetKeyDown(ExpKey))
        {
            bus.Publish(new ExpCollectedEvent(ExpAmount));
        }

        // 로그 구독자만 껐다 켜서, 해제된 구독자가 더 이상 호출되지 않는지 확인한다.
        if (Input.GetKeyDown(ToggleLogKey))
        {
            if (isLogSubscribed)
            {
                UnsubscribeLog();
            }
            else
            {
                SubscribeLog();
            }
        }
    }

    private void SubscribeLog()
    {
        bus.Subscribe<EnemyKilledEvent>(OnKilledLog);
        isLogSubscribed = true;
    }

    private void UnsubscribeLog()
    {
        bus.Unsubscribe<EnemyKilledEvent>(OnKilledLog);
        isLogSubscribed = false;
    }

    private void OnKilledScore(EnemyKilledEvent gameEvent)
    {
        score += gameEvent.ScoreValue;
    }

    private void OnKilledCount(EnemyKilledEvent gameEvent)
    {
        killCount++;
    }

    private void OnKilledLog(EnemyKilledEvent gameEvent)
    {
        DebugStats.Log($"[Observer] {gameEvent.MonsterName} killed +{gameEvent.ScoreValue:0}");
    }

    private void OnExpCollected(ExpCollectedEvent gameEvent)
    {
        exp += gameEvent.Amount;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20f, 60f, 520f, 90f),
            $"점수 {score:0} / 처치 {killCount} / 경험치 {exp:0}\n" +
            $"{KillKey}: 처치 발행, {ExpKey}: 경험치 발행, {ToggleLogKey}: 로그 구독 {(isLogSubscribed ? "ON" : "OFF")}");
    }
}
