using UnityEngine;
using UnityEngine.AI;
using SurvivalGame.Core;
using SurvivalGame.Player;

namespace SurvivalGame.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Dead
    }

    public class EnemyAI : MonoBehaviour
    {
        [Header("State & Target")]
        [SerializeField] private EnemyState currentState = EnemyState.Idle;
        [SerializeField] private Transform targetPlayer;

        [Header("Detection Ranges")]
        [SerializeField] private float sightRadius = 12f;
        [SerializeField] private float attackRadius = 1.8f;
        [SerializeField] private LayerMask detectionMask;

        [Header("Attack Settings")]
        [SerializeField] private float attackDamage = 18f;
        [SerializeField] private float attackCooldown = 1.4f;

        [Header("Movement")]
        [SerializeField] private float patrolSpeed = 2.0f;
        [SerializeField] private float chaseSpeed = 4.5f;
        [SerializeField] private float patrolRadius = 8f;

        private NavMeshAgent _agent;
        private EnemyHealth _health;
        private Vector3 _spawnPoint;
        private Vector3 _patrolTarget;
        private float _lastAttackTime;
        private float _stateTimer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<EnemyHealth>();
            _spawnPoint = transform.position;
        }

        private void Start()
        {
            if (targetPlayer == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) targetPlayer = player.transform;
            }

            PickRandomPatrolPoint();
        }

        private void Update()
        {
            if (GameLoop.Instance?.State != RunState.Running || (_health != null && _health.IsDead))
            {
                if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;
                return;
            }

            CheckForPlayer();

            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;
                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyState.Chase:
                    UpdateChase();
                    break;
                case EnemyState.Attack:
                    UpdateAttack();
                    break;
            }
        }

        private void CheckForPlayer()
        {
            if (targetPlayer == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= attackRadius)
            {
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer <= sightRadius)
            {
                currentState = EnemyState.Chase;
            }
            else if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
            {
                currentState = EnemyState.Patrol;
                PickRandomPatrolPoint();
            }
        }

        private void UpdateIdle()
        {
            _stateTimer += Time.deltaTime;
            if (_stateTimer >= 3.0f)
            {
                _stateTimer = 0f;
                PickRandomPatrolPoint();
                currentState = EnemyState.Patrol;
            }
        }

        private void UpdatePatrol()
        {
            MoveTowardsPosition(_patrolTarget, patrolSpeed);

            if (Vector3.Distance(transform.position, _patrolTarget) <= 1.2f)
            {
                currentState = EnemyState.Idle;
                _stateTimer = 0f;
            }
        }

        private void UpdateChase()
        {
            if (targetPlayer == null) return;
            MoveTowardsPosition(targetPlayer.position, chaseSpeed);
        }

        private void UpdateAttack()
        {
            if (targetPlayer == null) return;

            // Face target
            Vector3 lookDir = (targetPlayer.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 8f);
            }

            if (_agent != null && _agent.isOnNavMesh) _agent.isStopped = true;

            if (Time.time >= _lastAttackTime + attackCooldown)
            {
                _lastAttackTime = Time.time;
                PerformAttack();
            }

            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist > attackRadius)
            {
                currentState = EnemyState.Chase;
            }
        }

        private void PerformAttack()
        {
            if (targetPlayer.TryGetComponent<PlayerSurvivalStats>(out var stats))
            {
                float damage = attackDamage;
                if (targetPlayer.TryGetComponent<PlayerCombat>(out var combat))
                {
                    damage = combat.CalculateIncomingDamage(damage);
                }
                stats.TakeDamage(damage);
            }
        }

        private void MoveTowardsPosition(Vector3 destination, float speed)
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = false;
                _agent.speed = speed;
                _agent.SetDestination(destination);
            }
            else
            {
                // Fallback manual vector movement if NavMesh is not yet baked
                Vector3 direction = (destination - transform.position).normalized;
                direction.y = 0;
                transform.position += direction * (speed * Time.deltaTime);
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
                }
            }
        }

        private void PickRandomPatrolPoint()
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            _patrolTarget = _spawnPoint + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRadius);
        }
    }
}
