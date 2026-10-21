using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DashMonster : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float DashSpeed = 5f;
    public float DashRange = 10f;
    public float DashRecognizeRange = 5f;
    public float health = 50f;
    public float AttackDamage = 20f;
    public Transform target;
    public Vector3 direction;
    public Slider healthBar;
    public Coroutine DashCoroutine;

    private void Awake()
    {
        healthBar = GetComponentInChildren<Slider>();

        target = GameObject.FindGameObjectWithTag("Player").transform;

        // 체력바 위치 조정
        healthBar.transform.position = transform.position + Vector3.up * 1.5f;
    }

    // Player의 Transform 방향으로 이동
    public void Update()
    {
        if (health <= 0)
        {
            return;
        }

        direction = target.position - transform.position;

        Vector3 moveDirection = direction.normalized;

        // 플레이어와의 거리가 돌진 인식 범위보다 작아지면 돌진 수행
        if (direction.magnitude < DashRange && DashCoroutine == null)
        {
            DashCoroutine = StartCoroutine(Dash(moveDirection, direction.magnitude));
            return;
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        healthBar.value = health;
    }

    public IEnumerator Dash(Vector3 direction, float distance)
    {
        Vector3 endPosition = transform.position + direction * distance;

        yield return new WaitForSeconds(2f);

        // 돌진 거리만큼 이동하면 종료하도록
        while ((transform.position - endPosition).magnitude > 2f)
        {
            transform.Translate(direction * DashSpeed * Time.deltaTime);
            yield return null;
        }

        DashCoroutine = null;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // TODO : 플레이어 데미지 적용
        }
    }

    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        health -= 10;
    }
}