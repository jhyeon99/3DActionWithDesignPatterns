using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebuffMonster : MonoBehaviour
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
    [SerializeField] private float _castTime = 2f;
    [SerializeField] private float _castCoolTime = 6f;
    private float _elapsed = 0f;
    [SerializeField] private GameObject _expPrefab;

    private Coroutine _debuffCoroutine;

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

        _healthBar.transform.forward = Camera.main.transform.forward;

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

        _healthBar.value = _health / _maxHealth;

        transform.forward = (_target.position - transform.position).normalized;

        if (_elapsed >= _castCoolTime)
        {
            _elapsed = 0f;
            _debuffCoroutine = StartCoroutine(Debuff());
        }

        if (_debuffCoroutine == null)
        {
            _elapsed += Time.deltaTime;

            _direction = _target.position - transform.position;

            Vector3 moveDirection = _direction.normalized;
            moveDirection.y = 0f;

            transform.rotation = Quaternion.LookRotation(moveDirection);
            transform.Translate(
                Vector3.forward * _moveSpeed * Time.deltaTime,
                Space.Self
            );
        }
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

    private IEnumerator Debuff()
    {
        int randomDebuff = UnityEngine.Random.Range(1, 4);

        yield return new WaitForSeconds(_castTime);

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

        _elapsed = 0f;
        _debuffCoroutine = null;
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
