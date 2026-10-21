using System;
using UnityEngine;
using UnityEngine.UI;

public class NormalMonster : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float health = 100f;
    public float AttackDamage = 20f;
    public Transform target;
    public Vector3 direction;
    public Slider healthBar;

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

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        healthBar.value = health;
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