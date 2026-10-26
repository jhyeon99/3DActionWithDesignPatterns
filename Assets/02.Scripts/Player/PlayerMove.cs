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

    [Header("Components")]
    public CharacterController Controller;

    public Transform CameraTransform;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();

        CurrentSpeed = MoveSpeed;
    }


    private void Update()
    {
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

        Debug.Log($"Speed : {CurrentSpeed}");

        Vector3 velocity = direction * CurrentSpeed * Time.deltaTime;
        Controller.Move(velocity);
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