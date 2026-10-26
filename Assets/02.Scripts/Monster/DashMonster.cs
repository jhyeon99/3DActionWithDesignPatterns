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
    public float attackDamage = 20f;
    public float attackElapsed = 0f;
    public float attackInterval = 0.5f;
    public Transform target;
    public Vector3 direction;
    public Slider healthBar;
    public Coroutine DashCoroutine;
    public GameObject ExpPrefab;
    
    private void Awake()
    {
        healthBar = GetComponentInChildren<Slider>();

        // 체력바 위치 조정
        healthBar.transform.position = transform.position + Vector3.up * 1.5f;
    }

    // Player의 Transform 방향으로 이동
    public void Update()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (health <= 0)
        {
            int random = Random.Range(1, 5);
            
            for(int i = 0; i < random; i++)
            {
                Instantiate(ExpPrefab, transform.position, transform.rotation);
            }
            
            Destroy(gameObject);
            return;
        }
        
        healthBar.transform.forward = Camera.main.transform.forward;
        healthBar.value = health;

        direction = target.position - transform.position;

        Vector3 moveDirection = direction.normalized;
        moveDirection.y = 0f;

        // 플레이어와의 거리가 돌진 인식 범위보다 작아지면 돌진 수행
        if (direction.magnitude < DashRange && DashCoroutine == null)
        {
            DashCoroutine = StartCoroutine(Dash(moveDirection, direction.magnitude));
            return;
        }
        
        // 돌진 중에는 이동하지 않도록
        if (DashCoroutine != null)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(moveDirection);
        transform.Translate(
            Vector3.forward * moveSpeed * Time.deltaTime,
            Space.Self
        );
    }

    public IEnumerator Dash(Vector3 direction, float distance)
    {
        Debug.Log("저돌맹진!!");
        
        Vector3 endPosition = transform.position + direction * distance;
        Vector3 dashDirection = endPosition - transform.position;
        dashDirection.Normalize();
        
        yield return new WaitForSeconds(2f);

        // 돌진 거리만큼 이동하면 종료하도록
        while (Vector3.Distance(transform.position, endPosition) > 0.2f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                endPosition,
                DashSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        Debug.Log("저돌맹진 종료!!");
        
        DashCoroutine = null;
    }

    public void OnCollisionStay(Collision collision)
    {
        attackElapsed += Time.deltaTime;
        
        if (collision.gameObject.tag == "Player")
        {
            GameObject gameObject = collision.gameObject;

            if (gameObject != null && attackElapsed >= attackInterval)
            {
                Debug.Log("플레이어 공격!");
                gameObject.GetComponent<Player>().TakeDamage(attackDamage);
                attackElapsed = 0f;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    [ContextMenu("TakeDamage")]
    public void TestTakeDamage()
    {
        TakeDamage(10);
    }
}