using System;
using UnityEngine;
using UnityEngine.UI;

public class NormalMonster : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float health = 100f;
    public float attackDamage = 20f;
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
        moveDirection.y = 0f;

        transform.rotation = Quaternion.LookRotation(moveDirection);
        transform.Translate(
            Vector3.forward * moveSpeed * Time.deltaTime,
            Space.Self
        );

        healthBar.value = health;
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

    [ContextMenu("TakeDamage")]
    public void TakeDamage()
    {
        health -= 10;
    }
}