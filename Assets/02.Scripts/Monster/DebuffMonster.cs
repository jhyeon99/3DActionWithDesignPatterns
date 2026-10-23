using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebuffMonster : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float health = 100f;
    public float attackDamage = 20f;
    public Transform target;
    public Vector3 direction;
    public Slider healthBar;
    public float castTime = 2f;
    public float castCoolTime = 6f;
    public float elpased = 0f;

    public Coroutine debuffCoroutine;

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
        healthBar.transform.forward = Camera.main.transform.forward;
        
        if (health <= 0)
        {
            return;
        }
        
        healthBar.value = health;

        transform.forward = (target.position - transform.position).normalized;

        if (elpased >= castCoolTime)
        {
            elpased = 0f;
            debuffCoroutine = StartCoroutine(Debuff());
        }
        
        if(debuffCoroutine == null)
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

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject gameObject = collision.gameObject;

            if (gameObject != null)
            {
                Debug.Log(gameObject.name);
                gameObject.GetComponent<Player>().TakeDamage(attackDamage);
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
    
    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        health -= 10;
    }
}