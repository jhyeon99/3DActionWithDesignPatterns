using System;
using UnityEngine;

public class NormalMonster : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float health = 100f;
    public float AttackDamage = 20f;
    public Transform target;
    public Vector3 direction;

    // Player의 Transform 방향으로 이동
    public void Update()
    {
        direction = target.position - transform.position;

        Vector3 moveDirection = direction.normalized;

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            // TODO : 플레이어 데미지 적용
        }
    }
}