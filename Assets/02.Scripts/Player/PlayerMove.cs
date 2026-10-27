using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{
    private float DodgeTime = 0.5f;

    [Header("Stat")]
    public float MoveSpeed = 5f;

    public float DodgeSpeed = 30f;
    public float JumpSpeed = 10f; // 음 이거 왜있음? 
    public float CurrentSpeed = 0f;
    public bool IsDodge = false;
    public float MoveSpeedBonusRate = 0f; // 증강으로 늘어난 이동 속도 비율

    [Header("Components")]
    public CharacterController Controller;

    public Transform CameraTransform;

    private Player player;
    private float baseMoveSpeed;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        player = GetComponent<Player>();

        baseMoveSpeed = MoveSpeed;
        CurrentSpeed = MoveSpeed;
    }


    private void Update()
    {
        // 기절 중에는 이동/회피 불가
        if (player != null && player.IsStunned)
        {
            return;
        }

        // 1. 입력 받는다.
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // 2. 방향을 구한다.
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 카메라좌우 기준만 이동 방향으로 
        direction = Quaternion.Euler(0f, CameraTransform.eulerAngles.y, 0f) * direction;

        if (Input.GetButtonDown("Dodge"))
        {
            Debug.Log("Dodge Start");
            StartCoroutine(Dodge());
        }

        float slowRate = player != null && player.IsMoveSlowed ? player.MoveSlowRate : 1f;
        Vector3 velocity = direction * CurrentSpeed * slowRate * Time.deltaTime;
        Controller.Move(velocity);
    }

    // 증강: 이동 속도 +rate (기본값 대비 누적)
    public void UpgradeMoveSpeed(float rate)
    {
        MoveSpeedBonusRate += rate;
        MoveSpeed = baseMoveSpeed * (1f + MoveSpeedBonusRate);
        if (!IsDodge)
        {
            CurrentSpeed = MoveSpeed;
        }
    }

    public void ResetStats()
    {
        StopAllCoroutines();
        IsDodge = false;
        MoveSpeedBonusRate = 0f;
        MoveSpeed = baseMoveSpeed;
        CurrentSpeed = MoveSpeed;
    }

    private IEnumerator Dodge()
    {
        IsDodge = true;
        CurrentSpeed = DodgeSpeed;

        yield return new WaitForSeconds(DodgeTime);

        IsDodge = false;
        CurrentSpeed = MoveSpeed;
    }
}