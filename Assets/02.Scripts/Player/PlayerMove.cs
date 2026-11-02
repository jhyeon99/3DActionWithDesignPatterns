using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStat))]
public class PlayerMove : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform _cameraTransform;

    private float _currentSpeed = 0f;
    private bool _isDodge = false;

    private CharacterController _controller;
    private Player _player;
    private PlayerStat _stat;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<Player>();
        _stat = GetComponent<PlayerStat>();

        _currentSpeed = _stat.MoveSpeed;
    }

    private void Update()
    {
        // 기절 중에는 이동/회피 불가
        if (_player != null && _player.IsStunned)
        {
            return;
        }

        // 1. 입력 받는다.
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // 2. 방향을 구한다.
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 카메라좌우 기준만 이동 방향으로 
        direction = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f) * direction;

        if (Input.GetButtonDown("Dodge"))
        {
            Debug.Log("Dodge Start");
            StartCoroutine(Dodge());
        }

        float slowRate = _player != null && _player.IsMoveSlowed ? _stat.MoveSlowRate : 1f;
        Vector3 velocity = direction * _currentSpeed * slowRate * Time.deltaTime;
        _controller.Move(velocity);
    }

    public Transform GetCameraTransform()
    {
        if (_cameraTransform != null)
        {
            return _cameraTransform;
        }
        else
        {
            Debug.Log("CameraTrasform is null");
            return null;
        }
    }

    // 증강: 이동 속도 +rate (기본값 대비 누적)
    public void UpgradeMoveSpeed(float rate)
    {
        _stat.UpgradeMoveSpeed(rate);
        if (!_isDodge)
        {
            _currentSpeed = _stat.MoveSpeed;
        }
    }

    public void ResetStats()
    {
        StopAllCoroutines();
        _isDodge = false;
        _stat.ResetMoveBonus();
        _currentSpeed = _stat.MoveSpeed;
    }

    private IEnumerator Dodge()
    {
        _isDodge = true;
        _currentSpeed = _stat.DodgeSpeed;

        yield return new WaitForSeconds(_stat.DodgeTime);

        _isDodge = false;
        _currentSpeed = _stat.MoveSpeed;
    }
}