using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 몬스터 공통 부모 클래스 (Template Method 패턴)
/// Update / OnCollisionStay 의 처리 순서는 부모가 고정하고,
/// 몬스터마다 달라지는 Move / Attack 단계만 자식 클래스에서 override 하여 구현한다.
/// </summary>
public abstract class MonsterBase : MonoBehaviour
{
    [SerializeField] protected float _moveSpeed = 1.5f;
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _attackDamage = 20f;
    [SerializeField] protected float _attackInterval = 0.5f;
    [SerializeField] protected GameObject _expPrefab;

    protected float _health;
    protected Transform _target;
    protected Vector3 _direction;
    protected Slider _healthBar;

    private float _attackElapsed = 0f;

    protected virtual void Awake()
    {
        _health = _maxHealth;
        _healthBar = GetComponentInChildren<Slider>();

        // 체력바 위치 조정
        _healthBar.transform.position = transform.position + Vector3.up * 1.5f;
        _healthBar.value = _health / _maxHealth;
    }

    private void Update()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        if (_health <= 0)
        {
            Die();
            return;
        }

        UpdateHealthBar();

        _direction = _target.position - transform.position;
        Move();
    }

    private void OnCollisionStay(Collision collision)
    {
        _attackElapsed += Time.deltaTime;

        if (!collision.gameObject.CompareTag("Player")) return;
        if (_attackElapsed < _attackInterval) return;

        if (collision.gameObject.TryGetComponent(out Player player))
        {
            Attack(player);
            _attackElapsed = 0f;
        }
    }

    // ===== 자식 클래스가 구현해야 하는 단계 =====
    protected abstract void Move();
    protected abstract void Attack(Player player);

    // ===== 공통 처리 =====
    public void TakeDamage(float damage)
    {
        _health -= damage;
    }

    private void UpdateHealthBar()
    {
        _healthBar.transform.forward = Camera.main.transform.forward;
        _healthBar.value = _health / _maxHealth;
    }

    private void Die()
    {
        int random = Random.Range(1, 5);

        for (int i = 0; i < random; i++)
        {
            Instantiate(_expPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    // 자식 클래스에서 공용으로 사용하는 플레이어 추적 이동
    protected void MoveTowardTarget()
    {
        Vector3 moveDirection = _direction.normalized;
        moveDirection.y = 0f;

        transform.rotation = Quaternion.LookRotation(moveDirection);
        transform.Translate(
            Vector3.forward * _moveSpeed * Time.deltaTime,
            Space.Self
        );
    }

    [ContextMenu("TakeDamage")]
    private void TestTakeDamage()
    {
        TakeDamage(10);
    }
}