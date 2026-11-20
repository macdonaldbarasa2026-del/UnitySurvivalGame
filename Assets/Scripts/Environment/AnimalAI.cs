using UnityEngine;
using SurvivalGame.Core;

namespace SurvivalGame.Environment
{
    /// <summary>
    /// Lightweight wildlife behaviour: animals wander their territory and flee from the player.
    /// Kept intentionally simple so it is cheap on mobile GPUs and needs no animation assets.
    /// </summary>
    public class AnimalAI : MonoBehaviour
    {
        [SerializeField] private float wanderSpeed = 1.6f;
        [SerializeField] private float fleeSpeed = 4.6f;
        [SerializeField] private float fleeRadius = 9f;
        [SerializeField] private float roamRadius = 13f;

        private Vector3 _spawn;
        private Vector3 _target;
        private Transform _player;
        private float _thinkTimer;

        private void Start()
        {
            _spawn = transform.position;
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) _player = playerObject.transform;
            PickTarget();
        }

        private void Update()
        {
            if (GameLoop.Instance != null && GameLoop.Instance.State != RunState.Running) return;

            float distance = _player == null ? float.MaxValue : Vector3.Distance(transform.position, _player.position);
            bool fleeing = distance < fleeRadius;

            _thinkTimer -= Time.deltaTime;
            if (_thinkTimer <= 0f)
            {
                _thinkTimer = fleeing ? 0.4f : Random.Range(2f, 5f);
                if (fleeing && _player != null)
                {
                    Vector3 away = (transform.position - _player.position).normalized;
                    _target = transform.position + away * 8f;
                }
                else
                {
                    PickTarget();
                }
            }

            float speed = fleeing ? fleeSpeed : wanderSpeed;
            Vector3 direction = _target - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.2f)
            {
                direction.Normalize();
                transform.position += direction * (speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
            }

            float ground = WorldGenerator.Height(transform.position.x, transform.position.z);
            transform.position = new Vector3(transform.position.x, ground + 0.8f, transform.position.z);
        }

        private void PickTarget()
        {
            Vector2 offset = Random.insideUnitCircle * roamRadius;
            _target = _spawn + new Vector3(offset.x, 0f, offset.y);
        }
    }
}
