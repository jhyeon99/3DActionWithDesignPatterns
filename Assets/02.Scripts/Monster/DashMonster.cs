using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DashMonster : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _dashSpeed = 5f;
    [SerializeField] private float _dashRange = 10f;
    [SerializeField] private float _dashRecognizeRange = 5f;
    private float _health = 50f;
    [SerializeField] private float _maxHealth = 50f;
    [SerializeField] private float _attackDamage = 20f;
    private float _attackElapsed = 0f;
    [SerializeField] private float _attackInterval = 0.5f;
    private Transform _target;
    private Vector3 _direction;
    private Slider _healthBar;
    private Coroutine _dashCoroutine;
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
            int random = Random.Range(1, 5);

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

        // 플레이어와의 거리가 돌진 인식 범위보다 작아지면 돌진 수행
        if (_direction.magnitude < _dashRange && _dashCoroutine == null)
        {
            _dashCoroutine = StartCoroutine(Dash(moveDirection, _direction.magnitude));
            return;
        }

        // 돌진 중에는 이동하지 않도록
        if (_dashCoroutine != null)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(moveDirection);
        transform.Translate(
            Vector3.forward * _moveSpeed * Time.deltaTime,
            Space.Self
        );
    }

    private IEnumerator Dash(Vector3 direction, float distance)
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
                _dashSpeed * Time.deltaTime
            );
            yield return null;
        }

        Debug.Log("저돌맹진 종료!!");

        _dashCoroutine = null;
    }

    private void OnCollisionStay(Collision collision)
    {
        _attackElapsed += Time.deltaTime;

        if (collision.gameObject.tag == "Player")
        {
            GameObject gameObject = collision.gameObject;

            if (gameObject != null && _attackElapsed >= _attackInterval)
            {
                Debug.Log("플레이어 공격!");
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
        TakeDamage(10);
    }
}
