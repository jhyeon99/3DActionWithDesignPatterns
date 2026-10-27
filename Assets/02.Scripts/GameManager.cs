using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum UpgradeType
{
    AttackSpeed,
    Damage,
    MoveSpeed,
    HealthUp,
    Heal,
    DefenseUp
}

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// GameManager         시작 / 진행 / 게임 오버 상태 구현
    /// TimeManager         경과 시간 측정
    /// ExperienceManager   경험치 획득 및 레벨업
    /// ScoreManager        몬스터 처치 점수 계산
    /// WaveManager         일정 주기로 몬스터 스폰 요청
    /// UpgradeManager      증강 선택
    /// 몬스터 처치, 플레이어 사망 이벤트 연동
    /// </summary>
    [Header("Game Settings")]
    public float CurrentTime = 0f; // 진행 시간
    public int Level = 1; // 레벨
    public float Experience = 0f; // 경험치
    public float MaxExperience = 100f; // 레벨업 경험치
    public float Score = 0f; // 점수
    public int KillCount = 0; // 처치 수
    [Header("게임 시작 플래그")]
    public bool IsGameStarted = false;
    [Header("게임 진행 중 플래그")]
    public bool IsGaming = false;

    [Header("증강 UI")]
    [SerializeField] public GameObject UpgradePanel;
    [SerializeField] public Button[] UpgradeButtons;
    [SerializeField] public TMP_Text[] UpgradeLabels;

    [Header("게임 UI")]
    [SerializeField] public TextMeshProUGUI CurrentTimeText; // 경과 시간
    [SerializeField] public TextMeshProUGUI ExperienceText; // 경험치
    [SerializeField] public TextMeshProUGUI ScoreText; // 점수
    [SerializeField] public Slider ExperienceSlider; // 경험치 바
    [SerializeField] public GameObject GameOverPanel; // 게임 오버 화면
    [SerializeField] public GameObject PausePanel; // 일시 정지 화면
    [SerializeField] public TextMeshProUGUI LevelText; // 레벨

    [Header("몬스터 스폰")]
    [SerializeField] public GameObject[] MonsterPrefabs;
    [SerializeField] public Transform[] SpawnPoints;
    [SerializeField] public int SpawnCount = 3;

    [Header("스폰 위치 보정")]
    [Tooltip("스폰 포인트 주변으로 흩뿌리는 반경. 같은 지점에 겹쳐 쌓이는 것을 막는다.")]
    public float SpawnJitterRadius = 4f;
    [Tooltip("겹침/시야 검사에 실패했을 때 새 위치를 다시 뽑는 횟수")]
    public int SpawnRetryCount = 8;
    [Tooltip("카메라 시야 안에는 스폰하지 않는다. (모든 포인트가 시야 안이면 원래 포인트에 스폰)")]
    public bool SpawnOutsideView = true;
    [Tooltip("시야 판정 여유분(뷰포트 비율). 값이 클수록 더 멀리서 스폰한다.")]
    public float SpawnViewMargin = 0.1f;

    [Header("스폰 난이도")]
    public int InitialSpawnCount = 3;
    public float SpawnIncreaseInterval = 10f;

    public List<UpgradeType> UpgradeChoices = new();

    public float SpawnInterval = 5f;
    public float MonsterSpawnTimer = 0f;
    public int PendingUpgradeCount = 0;

    public static GameManager Instance { get; private set; }

    private Player player;
    private PlayerFire playerFire;
    private PlayerMove playerMove;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        CurrentTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(CurrentTime / 60f);
        int seconds = Mathf.FloorToInt(CurrentTime) % 60;
        CurrentTimeText.text = $"{minutes:00}:{seconds:00}";

        // 게임 일시 정지
        if (IsGameStarted && !UpgradePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (IsGaming)
                {
                    GamePause();
                    OpenPausePanel();
                }
                else
                {
                    GameResume();
                }
            }
        }

        // 게임 오버 후 재시작
        if (!IsGameStarted && Input.GetKeyDown(KeyCode.R))
        {
            RestartRun();
        }

        // Test용 경험치업
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ExperienceUp(50f);
        }

        // Test용 레벨업
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            LevelUp();
        }

        // Test용 스코어업
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ScoreUp(10f);
        }

        // 게임 오버: 플레이어 사망 시 (Test용: 4번 키로도 가능)
        if (IsGameStarted && ResolvePlayer() != null && player.IsDead)
        {
            GameOver();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameOver();
        }

        // 조건 || 몬스터의 수가 0일 때 추가
        if (IsGameStarted && IsGaming)
        {
            MonsterSpawnTimer += Time.deltaTime;
            if (MonsterSpawnTimer >= SpawnInterval)
            {
                MonsterWaveStart();
            }
        }
    }

    // 게임 시작
    public void GameStart()
    {
        // 게임 정보 초기화
        IsGameStarted = true;
        IsGaming = true;
        CurrentTime = 0f;
        Level = 1;
        Experience = 0f;
        MaxExperience = 100f;
        PendingUpgradeCount = 0;
        Score = 0f;
        KillCount = 0;
        Time.timeScale = 1;
        MonsterSpawnTimer = SpawnInterval;

        // 증강 패널 초기화
        UpgradeChoices.Clear();
        UpgradePanel.SetActive(false);

        // 게임 정보 UI 초기화
        CurrentTimeText.text = "";
        ExperienceText.text = $"{Experience:0} / {MaxExperience:0}";
        ScoreText.text = "0";
        LevelText.text = Level.ToString("0");

        // 경험치 초기화
        ExperienceSlider.minValue = 0f;
        ExperienceSlider.maxValue = MaxExperience;
        ExperienceSlider.value = Experience;
        Debug.Log("Game Start");

        // 게임 종료 패널 숨김
        GameOverPanel.SetActive(false);

        PausePanel.SetActive(false);
    }

    // 게임 종료
    public void GameOver()
    {
        GamePause();
        IsGameStarted = false;
        IsGaming = false;
        Debug.Log("Game Over");
        GameOverPanel.SetActive(true);
        UpgradePanel.SetActive(false);
        PausePanel.SetActive(false);
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 몬스터 처치 시 몬스터가 호출한다. (점수 + 처치 수)
    public void OnMonsterKilled(string monsterName, float scoreValue)
    {
        KillCount++;
        ScoreUp(scoreValue);
        DebugStats.Log($"{monsterName} killed +{scoreValue:0} score");
    }

    // 적 처치 시 사용
    public void ScoreUp(float value)
    {
        Score += value;
        ScoreText.text = Score.ToString("0");
    }

    // 적 처치 시 사용
    public void ExperienceUp(float value)
    {
        Experience += value;
        if (MaxExperience <= 0) return;
        while (Experience >= MaxExperience)
        {
            Experience -= MaxExperience;
            LevelUp();
        }

        ExperienceSlider.value = Experience;
        ExperienceText.text = $"{Experience:0} / {MaxExperience:0}";
    }

    public void LevelUp()
    {
        Level++;
        PendingUpgradeCount++;
        MaxExperience *= 1.1f;
        ExperienceSlider.maxValue = MaxExperience;
        ExperienceText.text = $"{Experience:0} / {MaxExperience:0}";
        LevelText.text = Level.ToString("0");
        if (!UpgradePanel.activeSelf && IsGaming)
        {
            OpenUpgradeSelection();
        }
    }

    public void ScoreDown(float value)
    {
        Score = Mathf.Max(0, Score - value);
        ScoreText.text = Score.ToString("0");
    }

    public void GamePause()
    {
        Time.timeScale = 0;
        IsGaming = false;
    }

    public void OpenPausePanel()
    {
        PausePanel.SetActive(true);
    }

    public void GameResume()
    {
        if (!IsGameStarted || UpgradePanel.activeSelf) return;
        Time.timeScale = 1;
        IsGaming = true;
        PausePanel.SetActive(false);
    }

    public void MonsterWaveStart()
    {
        // 주기적으로 스폰하는 몬스터 5마리씩 증가
        int increaseCount = Mathf.FloorToInt(CurrentTime / Mathf.Max(1f, SpawnIncreaseInterval)) * 5;
        SpawnCount = InitialSpawnCount + increaseCount;
        if (!IsGameStarted || !IsGaming)
            return;

        MonsterSpawnTimer = 0f;

        if (MonsterPrefabs == null || MonsterPrefabs.Length == 0 || SpawnPoints == null || SpawnPoints.Length == 0)
            return;

        for (int i = 0; i < SpawnCount; i++)
        {
            int monsterIndex = Random.Range(0, MonsterPrefabs.Length);
            int pointIndex = Random.Range(0, SpawnPoints.Length);

            SpawnMonster(monsterIndex, pointIndex);
        }
    }

    // 지정한 프리팹을 지정한 스폰 포인트에 생성한다. (웨이브/테스트 소환 공용)
    public GameObject SpawnMonster(int monsterIndex, int pointIndex)
    {
        if (MonsterPrefabs == null || SpawnPoints == null ||
            monsterIndex < 0 || monsterIndex >= MonsterPrefabs.Length ||
            pointIndex < 0 || pointIndex >= SpawnPoints.Length)
            return null;

        GameObject prefab = MonsterPrefabs[monsterIndex];
        Transform point = SpawnPoints[pointIndex];

        if (prefab == null || point == null)
            return null;
        return Instantiate(prefab, FindSpawnPosition(prefab, pointIndex), point.rotation);
    }

    private static readonly RaycastHit[] groundHits = new RaycastHit[16];

    // 스폰 포인트 주변의 지면 위에서, 시야 밖이고 다른 물체와 겹치지 않는 위치를 찾는다.
    // 포인트가 시야 안이면 다음 포인트를 시도하고, 모두 실패하면 원래 포인트의 지면 위치를 쓴다.
    private Vector3 FindSpawnPosition(GameObject prefab, int pointIndex)
    {
        // 프리팹의 캡슐 콜라이더로 발바닥 높이와 겹침 검사 크기를 구한다.
        float bottomOffset = 1f;
        float radius = 0.5f;
        float height = 2f;
        if (prefab.TryGetComponent(out CapsuleCollider capsule))
        {
            float scale = prefab.transform.localScale.y;
            radius = capsule.radius * scale;
            height = Mathf.Max(capsule.height * scale, radius * 2f);
            bottomOffset = height * 0.5f - capsule.center.y * scale;
        }

        for (int i = 0; i < SpawnPoints.Length; i++)
        {
            Transform point = SpawnPoints[(pointIndex + i) % SpawnPoints.Length];
            if (point == null)
                continue;

            for (int attempt = 0; attempt < Mathf.Max(1, SpawnRetryCount); attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * SpawnJitterRadius;
                Vector3 probe = new Vector3(point.position.x + offset.x, 0f, point.position.z + offset.y);

                if (!TryGetGroundY(probe, out float groundY))
                    continue;

                Vector3 position = new Vector3(probe.x, groundY + bottomOffset + 0.02f, probe.z);

                // 이 포인트가 시야 안이면 다음 포인트로 넘어간다.
                if (SpawnOutsideView && IsInCameraView(position))
                    break;

                float half = Mathf.Max(0f, height * 0.5f - radius);
                if (Physics.CheckCapsule(position + Vector3.down * half, position + Vector3.up * half, radius,
                        ~0, QueryTriggerInteraction.Ignore))
                    continue;

                return position;
            }
        }

        // 모두 실패: 원래 포인트의 지면 위에 그대로 스폰한다. (몬스터가 사라지지 않도록)
        Vector3 fallback = SpawnPoints[pointIndex].position;
        if (TryGetGroundY(fallback, out float fallbackGroundY))
            fallback.y = fallbackGroundY + bottomOffset + 0.02f;
        return fallback;
    }

    // 위에서 아래로 쏜 레이 중 가장 낮은 고정 콜라이더의 높이를 지면으로 본다.
    // (장애물 윗면이나 몬스터/플레이어 위를 지면으로 착각하지 않도록 리지드바디·캐릭터 컨트롤러는 제외)
    private static bool TryGetGroundY(Vector3 xz, out float groundY)
    {
        groundY = 0f;
        int count = Physics.RaycastNonAlloc(new Vector3(xz.x, 100f, xz.z), Vector3.down, groundHits, 200f, ~0,
            QueryTriggerInteraction.Ignore);

        bool found = false;
        float lowest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider hitCollider = groundHits[i].collider;
            if (hitCollider.attachedRigidbody != null || hitCollider is CharacterController)
                continue;

            if (groundHits[i].point.y < lowest)
            {
                lowest = groundHits[i].point.y;
                found = true;
            }
        }

        if (found)
            groundY = lowest;
        return found;
    }

    private bool IsInCameraView(Vector3 worldPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return false;

        Vector3 viewport = mainCamera.WorldToViewportPoint(worldPosition);
        return viewport.z > 0f &&
               viewport.x > -SpawnViewMargin && viewport.x < 1f + SpawnViewMargin &&
               viewport.y > -SpawnViewMargin && viewport.y < 1f + SpawnViewMargin;
    }

    // 살아 있는 몬스터를 모두 제거한다.
    public void ClearMonsters()
    {
        foreach (var monster in FindObjectsByType<NormalMonster>(FindObjectsSortMode.None)) Destroy(monster.gameObject);
        foreach (var monster in FindObjectsByType<DashMonster>(FindObjectsSortMode.None)) Destroy(monster.gameObject);
        foreach (var monster in FindObjectsByType<DebuffMonster>(FindObjectsSortMode.None)) Destroy(monster.gameObject);
    }

    // 씬을 다시 불러오지 않고 새 런을 시작한다. (이 오브젝트는 DontDestroyOnLoad)
    public void RestartRun()
    {
        ClearMonsters();
        foreach (var exp in FindObjectsByType<Exp>(FindObjectsSortMode.None)) Destroy(exp.gameObject);
        foreach (var bullet in FindObjectsByType<PlayerBullet>(FindObjectsSortMode.None)) Destroy(bullet.gameObject);

        if (ResolvePlayer() != null) player.ResetForNewRun();
        if (playerFire == null) playerFire = FindFirstObjectByType<PlayerFire>();
        if (playerFire != null) playerFire.ResetStats();
        if (playerMove == null) playerMove = FindFirstObjectByType<PlayerMove>();
        if (playerMove != null) playerMove.ResetStats();

        DebugStats.Log("Run restarted");
        GameStart();
    }

    private Player ResolvePlayer()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        return player;
    }

    // 증강 패널 오픈 (랜덤 증강 선정 후 오픈)
    public void OpenUpgradeSelection()
    {
        IsGaming = false;
        GamePause();
        var pool = new List<UpgradeType>
        {
            UpgradeType.AttackSpeed,
            UpgradeType.Damage,
            UpgradeType.MoveSpeed,
            UpgradeType.Heal,
            UpgradeType.HealthUp,
            UpgradeType.DefenseUp
        };

        UpgradeChoices.Clear();

        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, pool.Count);

            UpgradeChoices.Add(pool[index]);
            pool.RemoveAt(index);
        }

        for (int i = 0; i < 3; i++)
        {
            int choiceIndex = i;
            UpgradeLabels[i].text = GetUpgradeDescription(UpgradeChoices[i]);
            UpgradeButtons[i].onClick.RemoveAllListeners();
            UpgradeButtons[i].onClick.AddListener(() => SelectUpgrade(choiceIndex));
        }

        UpgradePanel.SetActive(true);
    }

    // 증강 Text return
    public string GetUpgradeDescription(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.AttackSpeed:
                return "속사\n공격 속도 +15%";
            case UpgradeType.Damage:
                return "화력 강화\n공격력 + 20%";
            case UpgradeType.MoveSpeed:
                return "신속\n이동 속도 +15%";
            case UpgradeType.Heal:
                return "치료\n체력 30 회복";
            case UpgradeType.HealthUp:
                return "체력 증가\n최대 체력 10 증가";
            case UpgradeType.DefenseUp:
                return "방어력 증가\n방어력 10% 증가";
            default:
                return "";
        }
    }

    // 증강 선택
    public void SelectUpgrade(int choiceIndex)
    {
        if (!IsGameStarted || !UpgradePanel.activeSelf || choiceIndex < 0 || choiceIndex >= UpgradeChoices.Count ||
            PendingUpgradeCount <= 0)
            return;
        ApplyUpgrade(UpgradeChoices[choiceIndex]);
        PendingUpgradeCount--;
        if (PendingUpgradeCount > 0)
        {
            OpenUpgradeSelection();
        }
        else
        {
            UpgradePanel.SetActive(false);
            GameResume();
        }
    }

    // 증강 적용
    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        ResolvePlayer();
        if (playerFire == null) playerFire = FindFirstObjectByType<PlayerFire>();
        if (playerMove == null) playerMove = FindFirstObjectByType<PlayerMove>();

        switch (upgradeType)
        {
            case UpgradeType.AttackSpeed:
                if (playerFire != null) playerFire.UpgradeAttackSpeed(0.15f);
                Debug.Log("공격 속도 증가");
                break;

            case UpgradeType.Damage:
                if (playerFire != null) playerFire.UpgradeDamage(0.2f);
                Debug.Log("공격력 증가");
                break;

            case UpgradeType.MoveSpeed:
                if (playerMove != null) playerMove.UpgradeMoveSpeed(0.15f);
                Debug.Log("이동 속도 증가");
                break;

            case UpgradeType.Heal:
                if (player != null) player.UpgradeHeal(30f);
                Debug.Log("체력 회복");
                break;

            case UpgradeType.HealthUp:
                if (player != null) player.UpgradeMaxHp(10f);
                Debug.Log("최대 체력 증가");
                break;
            case UpgradeType.DefenseUp:
                if (player != null) player.UpgradeDefense(10f);
                Debug.Log("방어력 증가");
                break;
        }

        DebugStats.Log($"Upgrade applied: {upgradeType}");
    }
}