using UnityEngine;

public class Exp : MonoBehaviour
{
    [Header("Magnet")]
    [Tooltip("플레이어가 이 거리 안에 들어오면 경험치가 따라갑니다.")]
    [Min(0f)]
    [SerializeField] private float _magnetRange = 5f;

    [Tooltip("플레이어에게 도달하는 데 걸리는 시간입니다.")]
    [Min(0.01f)]
    [SerializeField] private float _curveDuration = 0.6f;

    [Tooltip("베지어 곡선의 높이입니다.")]
    [SerializeField] private float _curveHeight = 1.5f;

    [Tooltip("이 거리 안에 들어오면 경험치를 획득한 것으로 처리하고 제거합니다.")]
    [Min(0.01f)]
    [SerializeField] private float _collectDistance = 0.2f;

    private Transform _player;
    private Vector3 _curveStart;
    private Vector3 _curveControl;
    private Vector3 _curveEnd;
    private float _curveProgress;
    private bool _isAttracted;


    private void Update()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;

        // 경험치가 플레이어를 따라가는 중이 아니라면
        if (!_isAttracted)
        {
            // 플레이어와의 거리를 계산하여 _magnetRange보다 멀면 return
            if (Vector3.Distance(transform.position, _player.position) > _magnetRange)
            {
                return;
            }

            // _magnetRange보다 가까우면 추적 시작
            BeginAttraction();
        }

        // 플레이어의 경험치 획득 범위 안에 도달하면 Destroy
        if (Vector3.Distance(transform.position, _player.position) <= _collectDistance)
        {
            transform.position = _player.position;
            Destroy(gameObject);
            return;
        }

        MoveAlongBezierCurve();
    }


    // 플레이어 추적을 시작한다.
    private void BeginAttraction()
    {
        _curveStart = transform.position;
        _curveEnd = _player.position;

        Vector3 midpoint = Vector3.Lerp(_curveStart, _curveEnd, 0.5f);
        _curveControl = midpoint + Vector3.up * _curveHeight;

        _curveProgress = 0f;
        _isAttracted = true;
    }

    // 끝 점을 플레이어 위치로 하여 베지어 곡선을 그리며 이동
    private void MoveAlongBezierCurve()
    {
        // 플레이어가 움직여도 곡선의 끝점이 플레이어를 계속 바라보도록 갱신한다.
        _curveEnd = _player.position;

        _curveProgress += Time.deltaTime / Mathf.Max(0.01f, _curveDuration);
        float t = Mathf.Clamp01(_curveProgress);

        // 2차 베지어 곡선: 시작점, 제어점, 끝점
        float inverseT = 1f - t;
        transform.position = inverseT * inverseT * _curveStart
                             + 2f * inverseT * t * _curveControl
                             + t * t * _curveEnd;

        if (t >= 1f)
        {
            transform.position = _curveEnd;
            Destroy(gameObject);
        }
    }
}
