using System;
using UnityEngine;
using UnityEngine.UI;

public class NormalMonster : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.5f;
    private float _health = 100f;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _attackDamage = 20f;
    private float _attackElapsed = 0f;
    [SerializeField] private float _attackInterval = 0.5f;
    private Transform _target;
    private Vector3 _direction;
    private Slider _healthBar;
    [SerializeField] private GameObject _expPrefab;

    private void Awake()
    {
        _health = _maxHealth;
        _healthBar = GetComponentInChildren<Slider>();

        // 체력바 위치 조정
        _healthBar.transform.position = transform.position + Vector3.up * 1.5f;
        _healthBar.value = _health / _maxHealth;
    }

    // Player의 Transform 방향으로 이동
    private void Update()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        if (_health <= 0)
        {
            int random = UnityEngine.Random.Range(1, 5);

            for (int i = 0; i < random; i++)
            {
                Instantiate(_expPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
            return;
        }

        _healthBar.transform.forward = Camera.main.transform.forward;
        _healthBar.value = _health / _maxHealth;

        _direction = _target.position - transform.position;

        Vector3 moveDirection = _direction.normalized;
        moveDirection.y = 0f;

        transform.rotation = Quaternion.LookRotation(moveDirection);
        transform.Translate(
            Vector3.forward * _moveSpeed * Time.deltaTime,
            Space.Self
        );
    }

    private void OnCollisionStay(Collision collision)
    {
        _attackElapsed += Time.deltaTime;

        if (collision.gameObject.tag == "Player")
        {
            GameObject gameObject = collision.gameObject;

            if (gameObject != null && _attackElapsed >= _attackInterval)
            {
                Debug.Log(gameObject.name);
                gameObject.GetComponent<Player>().TakeDamage(_attackDamage);
                _attackElapsed = 0f;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
    }

    [ContextMenu("TakeDamage")]
    private void TestTakeDamage()
    {
        _health -= 10;
    }
}
