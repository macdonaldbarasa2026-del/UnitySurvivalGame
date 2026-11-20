using System;
using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Inventory;
using SurvivalGame.Environment;

namespace SurvivalGame.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Melee Combat Settings")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRange = 1.6f;
        [SerializeField] private float attackRadius = 0.6f;
        [SerializeField] private float baseDamage = 15f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackStaminaCost = 12f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Defense / Blocking")]
        [SerializeField] private float blockDamageReduction = 0.65f;
        [SerializeField] private float blockStaminaDrain = 10f;

        private PlayerSurvivalStats _stats;
        private float _lastAttackTime;
        private bool _isBlocking;

        public bool IsBlocking => _isBlocking;
        public event Action OnAttackExecuted;
        public event Action<bool> OnBlockStateChanged;

        private void Awake()
        {
            _stats = GetComponent<PlayerSurvivalStats>();
            if (hitLayers == 0) hitLayers = ~0;
            if (attackPoint == null)
            {
                var point = new GameObject("AttackPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = new Vector3(0f, 1.0f, 1.0f);
                attackPoint = point.transform;
            }
        }

        private void Update()
        {
            if (GameLoop.Instance?.State != RunState.Running || (_stats != null && _stats.IsDead))
            {
                return;
            }

            HandleBlock();
            HandleAttack();
        }

        private void HandleBlock()
        {
            bool wantsBlock = GameInput.Block;
            if (wantsBlock != _isBlocking)
            {
                _isBlocking = wantsBlock;
                OnBlockStateChanged?.Invoke(_isBlocking);
            }
        }

        private void HandleAttack()
        {
            if (_isBlocking) return;

            if (GameInput.AttackPressed && Time.time >= _lastAttackTime + attackCooldown)
            {
                if (_stats != null && !_stats.UseStamina(attackStaminaCost))
                {
                    return; // Exhausted
                }

                _lastAttackTime = Time.time;
                ExecuteAttack();
            }
        }

        private void ExecuteAttack()
        {
            OnAttackExecuted?.Invoke();
            SurvivalGame.Audio.SoundManager.Instance?.PlayAttackSwing();

            Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, hitLayers);
            bool hitSomething = false;
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                Vector3 hitPoint = hit.ClosestPoint(attackPoint.position);
                Vector3 hitNormal = (attackPoint.position - hitPoint).normalized;

                // 1. Damage Enemies / Entities
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(baseDamage, hitPoint, hitNormal);
                    hitSomething = true;
                }

                // 2. Harvest Natural Resources (Trees, Rocks)
                if (hit.TryGetComponent<HarvestableResource>(out var resource))
                {
                    resource.Hit(baseDamage, hitPoint);
                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                SurvivalGame.Audio.SoundManager.Instance?.PlayHitImpact();
            }
        }

        public float CalculateIncomingDamage(float incomingDamage)
        {
            if (_isBlocking)
            {
                if (_stats != null && _stats.UseStamina(blockStaminaDrain))
                {
                    return incomingDamage * (1f - blockDamageReduction);
                }
            }
            return incomingDamage;
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
            }
        }
    }
}
