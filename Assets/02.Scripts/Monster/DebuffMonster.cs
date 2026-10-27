using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebuffMonster : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float health = 100f;
    public float maxHealth = 100f;
    public float attackDamage = 20f;
    public float attackElapsed = 0f;
    public float attackInterval = 0.5f;
    public Transform target;
    public Vector3 direction;
    public Slider healthBar;
    public float castTime = 2f;
    public float castCoolTime = 6f;
    public float elpased = 0f;
    public GameObject ExpPrefab;

    public Coroutine debuffCoroutine;

    private void Awake()
    {
        health = maxHealth;

        healthBar = GetComponentInChildren<Slider>();

        // 체력바 위치 조정
        healthBar.transform.position = transform.position + Vector3.up * 1.5f;
        healthBar.value = health / maxHealth;
    }

    // Player의 Transform 방향으로 이동
    public void Update()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        healthBar.transform.forward = Camera.main.transform.forward;

        if (health <= 0)
        {
            int random = UnityEngine.Random.Range(1, 5);

            for (int i = 0; i < random; i++)
            {
                Instantiate(ExpPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
            return;
        }

        healthBar.value = health / maxHealth;

        transform.forward = (target.position - transform.position).normalized;

        if (elpased >= castCoolTime)
        {
            elpased = 0f;
            debuffCoroutine = StartCoroutine(Debuff());
        }

        if (debuffCoroutine == null)
        {
            elpased += Time.deltaTime;

            direction = target.position - transform.position;

            Vector3 moveDirection = direction.normalized;
            moveDirection.y = 0f;

            transform.rotation = Quaternion.LookRotation(moveDirection);
            transform.Translate(
                Vector3.forward * moveSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        attackElapsed += Time.deltaTime;

        if (collision.gameObject.tag == "Player")
        {
            GameObject gameObject = collision.gameObject;

            if (gameObject != null && attackElapsed >= attackInterval)
            {
                Debug.Log(gameObject.name);
                gameObject.GetComponent<Player>().TakeDamage(attackDamage);
                attackElapsed = 0f;
            }
        }
    }

    public IEnumerator Debuff()
    {
        int randomDebuff = UnityEngine.Random.Range(1, 4);

        yield return new WaitForSeconds(castTime);

        switch (randomDebuff)
        {
            case 1: // 이속 감소
                Debug.Log("이감!");
                break;
            case 2: // 공속 감소
                Debug.Log("공감!");
                break;
            case 3: // 기절
                Debug.Log("기절!");
                break;
        }

        elpased = 0f;
        debuffCoroutine = null;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    [ContextMenu("TakeDamage")]
    public void TestTakeDamage()
    {
        health -= 10;
    }
}