using UnityEngine;

public class Exp : MonoBehaviour
{
    [Header("Magnet")]
    [Tooltip("플레이어가 이 거리 안에 들어오면 경험치가 따라갑니다.")]
    [Min(0f)]
    public float magnetRange = 5f;

    [Tooltip("플레이어에게 도달하는 데 걸리는 시간입니다.")]
    [Min(0.01f)]
    public float curveDuration = 0.6f;

    [Tooltip("베지어 곡선의 높이입니다.")]
    public float curveHeight = 1.5f;

    [Tooltip("이 거리 안에 들어오면 경험치를 획득한 것으로 처리하고 제거합니다.")]
    [Min(0.01f)]
    public float collectDistance = 0.2f;

    private Transform player;
    private Vector3 curveStart;
    private Vector3 curveControl;
    private Vector3 curveEnd;
    private float curveProgress;
    private bool isAttracted;


    private void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 경험치가 플레이어를 따라가는 중이 아니라면
        if (!isAttracted)
        {
            // 플레이어와의 거리를 계산하여 magnetRange보다 멀면 return
            if (Vector3.Distance(transform.position, player.position) > magnetRange)
            {
                return;
            }

            // magnetRange보다 가까우면 추적 시작
            BeginAttraction();
        }

        // 플레이어의 경험치 획득 범위 안에 도달하면 Destroy 
        if (Vector3.Distance(transform.position, player.position) <= collectDistance)
        {
            transform.position = player.position;
            Destroy(gameObject);
            return;
        }

        MoveAlongBezierCurve();
    }


    // 플레이어 추적을 시작한다.
    private void BeginAttraction()
    {
        curveStart = transform.position;
        curveEnd = player.position;

        Vector3 midpoint = Vector3.Lerp(curveStart, curveEnd, 0.5f);
        curveControl = midpoint + Vector3.up * curveHeight;

        curveProgress = 0f;
        isAttracted = true;
    }

    // 끝 점을 플레이어 위치로 하여 베지어 곡선을 그리며 이동
    private void MoveAlongBezierCurve()
    {
        // 플레이어가 움직여도 곡선의 끝점이 플레이어를 계속 바라보도록 갱신한다.
        curveEnd = player.position;

        curveProgress += Time.deltaTime / Mathf.Max(0.01f, curveDuration);
        float t = Mathf.Clamp01(curveProgress);

        // 2차 베지어 곡선: 시작점, 제어점, 끝점
        float inverseT = 1f - t;
        transform.position = inverseT * inverseT * curveStart
                             + 2f * inverseT * t * curveControl
                             + t * t * curveEnd;

        if (t >= 1f)
        {
            transform.position = curveEnd;
            Destroy(gameObject);
        }
    }
}