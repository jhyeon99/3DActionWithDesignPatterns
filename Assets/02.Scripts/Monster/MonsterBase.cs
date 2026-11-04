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

    private float _lastAttackTime = float.NegativeInfinity;

    protected virtual void Awake()
    {
        _health = _maxHealth;
        _healthBar = GetComponentInChildren<Slider>();

        // 체력바 위치 조정
        _healthBar.transform.position = transform.position + Vector3.up * 1.5f;
        _healthBar.value = _health / _maxHealth;
    }

    private void Start()
    {
        FindTarget();
    }

    private void Update()
    {
        if (_health <= 0)
        {
            Die();
            return;
        }

        UpdateHealthBar();

        // 플레이어가 없으면 이동하지 않도록
        if (_target == null && !FindTarget())
        {
            return;
        }

        _direction = _target.position - transform.position;
        Move();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (Time.time - _lastAttackTime < _attackInterval) return;

        if (collision.gameObject.TryGetComponent(out Player player))
        {
            Attack(player);
            _lastAttackTime = Time.time;
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

    // Player 태그 오브젝트를 찾아 캐싱, 찾으면 true
    private bool FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _target = player != null ? player.transform : null;
        return _target != null;
    }

    private void UpdateHealthBar()
    {
        _healthBar.transform.forward = Camera.main.transform.forward;
        _healthBar.value = _health / _maxHealth;
    }

    private void Die()
    {
        if (_expPrefab != null)
        {
            int random = Random.Range(1, 5);

            for (int i = 0; i < random; i++)
            {
                Instantiate(_expPrefab, transform.position, transform.rotation);
            }
        }
        else
        {
            Debug.LogWarning($"{name}: Exp 프리팹이 연결되지 않았습니다.");
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