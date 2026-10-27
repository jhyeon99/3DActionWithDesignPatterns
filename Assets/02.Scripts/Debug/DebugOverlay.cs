using System;
using System.Text;
using UnityEngine;

/// <summary>
/// 테스트/시각화용 오버레이. 씬에 배치하지 않아도 실행 시 자동 생성된다.
/// F1 로 켜고 끈다. 측정할 때는 오버레이 자체의 비용을 빼기 위해 끄고 비교한다.
///
/// F2 일반 / F3 돌진 / F4 디버프 몬스터 5마리 소환
/// F5 몬스터 50마리 소환(부하 테스트)   F9 몬스터 전체 제거
/// F6 관통 / F7 치명타 / F8 공속 버프 토글
/// </summary>
public class DebugOverlay : MonoBehaviour
{
    private const int GraphSamples = 120;
    private const float GraphMaxMs = 50f;
    private const float RefreshInterval = 0.5f;

    private readonly float[] frameMs = new float[GraphSamples];
    private readonly StringBuilder builder = new();

    private int frameIndex;
    private float sampleTimer;
    private int sampleFrames;
    private float worstMs;
    private float fps;
    private float shownWorstMs;
    private float gcPerSecond;
    private int lastGcCount;

    private bool visible = true;
    private string statsText = "";
    private string logText = "";

    private Player player;
    private PlayerFire playerFire;
    private PlayerMove playerMove;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (FindFirstObjectByType<DebugOverlay>() != null)
        {
            return;
        }

        GameObject overlay = new("DebugOverlay");
        DontDestroyOnLoad(overlay);
        overlay.AddComponent<DebugOverlay>();
    }

    private void Update()
    {
        float deltaMs = Time.unscaledDeltaTime * 1000f;
        frameMs[frameIndex] = deltaMs;
        frameIndex = (frameIndex + 1) % GraphSamples;

        sampleTimer += Time.unscaledDeltaTime;
        sampleFrames++;
        worstMs = Mathf.Max(worstMs, deltaMs);

        if (sampleTimer >= RefreshInterval)
        {
            int gcCount = GC.CollectionCount(0);
            fps = sampleFrames / sampleTimer;
            gcPerSecond = (gcCount - lastGcCount) / sampleTimer;
            shownWorstMs = worstMs;

            lastGcCount = gcCount;
            sampleTimer = 0f;
            sampleFrames = 0;
            worstMs = 0f;

            ResolveReferences();
            BuildTexts();
        }

        HandleHotkeys();
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (playerFire == null)
        {
            playerFire = FindFirstObjectByType<PlayerFire>();
        }

        if (playerMove == null)
        {
            playerMove = FindFirstObjectByType<PlayerMove>();
        }
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            visible = !visible;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            if (Input.GetKeyDown(KeyCode.F2)) SpawnMonsters(gameManager, 0, 5);
            if (Input.GetKeyDown(KeyCode.F3)) SpawnMonsters(gameManager, 1, 5);
            if (Input.GetKeyDown(KeyCode.F4)) SpawnMonsters(gameManager, 2, 5);
            if (Input.GetKeyDown(KeyCode.F5)) SpawnMonsters(gameManager, -1, 50);
            if (Input.GetKeyDown(KeyCode.F9)) gameManager.ClearMonsters();
        }

        if (playerFire != null)
        {
            if (Input.GetKeyDown(KeyCode.F6)) playerFire.IsPierceBuff = !playerFire.IsPierceBuff;
            if (Input.GetKeyDown(KeyCode.F7)) playerFire.IsCritBuff = !playerFire.IsCritBuff;
            if (Input.GetKeyDown(KeyCode.F8)) playerFire.IsAttackSpeedBuff = !playerFire.IsAttackSpeedBuff;
        }
    }

    // monsterIndex가 -1이면 종류를 섞어서 소환한다.
    private static void SpawnMonsters(GameManager gameManager, int monsterIndex, int count)
    {
        if (gameManager.MonsterPrefabs == null || gameManager.MonsterPrefabs.Length == 0 ||
            gameManager.SpawnPoints == null || gameManager.SpawnPoints.Length == 0)
        {
            DebugStats.Log("Spawn failed: prefabs/spawn points not set");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            int index = monsterIndex >= 0 ? monsterIndex : UnityEngine.Random.Range(0, gameManager.MonsterPrefabs.Length);
            gameManager.SpawnMonster(index, UnityEngine.Random.Range(0, gameManager.SpawnPoints.Length));
        }

        DebugStats.Log($"Debug spawn x{count}");
    }

    private void BuildTexts()
    {
        builder.Clear();
        builder.Append("FPS ").Append(fps.ToString("0.0"))
            .Append("   worst ").Append(shownWorstMs.ToString("0.0")).Append(" ms")
            .Append("   GC/s ").Append(gcPerSecond.ToString("0.0")).Append('\n');
        builder.Append("Mono ").Append((GC.GetTotalMemory(false) / 1048576f).ToString("0.0")).Append(" MB\n");

        builder.Append("\n[Objects]  active / created / destroyed\n");
        AppendKind(StatKind.Bullet);
        AppendKind(StatKind.NormalMonster);
        AppendKind(StatKind.DashMonster);
        AppendKind(StatKind.DebuffMonster);
        AppendKind(StatKind.Exp);

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            builder.Append("\n[Run]  Lv ").Append(gameManager.Level)
                .Append("  Exp ").Append(gameManager.Experience.ToString("0")).Append('/').Append(gameManager.MaxExperience.ToString("0"))
                .Append("\nScore ").Append(gameManager.Score.ToString("0"))
                .Append("  Kills ").Append(gameManager.KillCount)
                .Append("  Wave size ").Append(gameManager.SpawnCount).Append('\n');
        }

        if (player != null)
        {
            builder.Append("\n[Player]  HP ").Append(player.CurrentHp.ToString("0.#")).Append('/').Append(player.MaxHp.ToString("0.#"))
                .Append("  Def ").Append(player.DefensePercent.ToString("0.#")).Append("%\n");
        }

        if (playerFire != null)
        {
            builder.Append("Dmg ").Append(playerFire.Damage.ToString("0.0"))
                .Append("  AtkSpd x").Append(playerFire.AttackSpeedBonus.ToString("0.00"));
            if (playerMove != null)
            {
                builder.Append("  MoveSpd ").Append(playerMove.MoveSpeed.ToString("0.0"));
            }

            builder.Append("\nBuff  Pierce ").Append(playerFire.IsPierceBuff ? "ON" : "off")
                .Append("  Crit ").Append(playerFire.IsCritBuff ? "ON" : "off")
                .Append("  AtkSpd ").Append(playerFire.IsAttackSpeedBuff ? "ON" : "off").Append('\n');
        }

        if (player != null)
        {
            builder.Append("Debuff  ");
            if (player.StunTimer <= 0f && player.MoveSlowTimer <= 0f && player.AttackSlowTimer <= 0f)
            {
                builder.Append("none");
            }

            if (player.StunTimer > 0f) builder.Append("Stun ").Append(player.StunTimer.ToString("0.0")).Append("s  ");
            if (player.MoveSlowTimer > 0f) builder.Append("MoveSlow ").Append(player.MoveSlowTimer.ToString("0.0")).Append("s  ");
            if (player.AttackSlowTimer > 0f) builder.Append("AtkSlow ").Append(player.AttackSlowTimer.ToString("0.0")).Append("s  ");
            builder.Append('\n');
        }

        statsText = builder.ToString();

        builder.Clear();
        for (int i = 0; i < DebugStats.LogCount; i++)
        {
            builder.Append(DebugStats.GetLog(i)).Append('\n');
        }

        logText = builder.ToString();
    }

    private void AppendKind(StatKind kind)
    {
        int index = (int)kind;
        builder.Append(kind).Append("  ")
            .Append(DebugStats.Active(kind)).Append(" / ")
            .Append(DebugStats.Created[index]).Append(" / ")
            .Append(DebugStats.Destroyed[index]).Append('\n');
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        const float x = 20f;
        const float width = 360f;

        GUIStyle style = GUI.skin.label;
        style.fontSize = 14;
        style.richText = false;

        float statsHeight = 300f;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(x - 8f, 60f, width, statsHeight + 80f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, 64f, width - 16f, statsHeight), statsText, style);

        DrawFrameGraph(new Rect(x, 64f + statsHeight, width - 16f, 60f));

        // 전투 로그 (좌하단)
        float logHeight = 160f;
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(x - 8f, Screen.height - logHeight - 12f, 520f, logHeight), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(x, Screen.height - logHeight - 8f, 504f, logHeight - 8f), logText, style);

        GUI.Label(new Rect(Screen.width - 330f, Screen.height - 130f, 320f, 130f),
            "F1 Overlay   F2/F3/F4 Spawn x5\nF5 Spawn x50   F9 Clear monsters\nF6 Pierce  F7 Crit  F8 AtkSpd buff\n" +
            "1 Exp+50  2 LevelUp  3 Score+10  4 GameOver\nH Heal  J Damage  R Restart (GameOver)", style);
    }

    private void DrawFrameGraph(Rect rect)
    {
        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        float barWidth = rect.width / GraphSamples;
        for (int i = 0; i < GraphSamples; i++)
        {
            float ms = frameMs[(frameIndex + i) % GraphSamples];
            float height = Mathf.Clamp01(ms / GraphMaxMs) * rect.height;

            GUI.color = ms < 16.7f ? Color.green : ms < 33.4f ? Color.yellow : Color.red;
            GUI.DrawTexture(new Rect(rect.x + i * barWidth, rect.yMax - height, Mathf.Max(1f, barWidth - 0.5f), height),
                Texture2D.whiteTexture);
        }

        // 60fps(16.7ms) 기준선
        GUI.color = new Color(1f, 1f, 1f, 0.6f);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 16.7f / GraphMaxMs * rect.height, rect.width, 1f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
