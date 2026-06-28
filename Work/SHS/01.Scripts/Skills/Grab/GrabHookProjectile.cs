using Code.SHS.Entities.Enemies.Combat;
using Cysharp.Threading.Tasks;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.SkillSystem.Skills.Grab
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class GrabHookProjectile : MonoBehaviour
    {
        public struct Config
        {
            public LayerMask HitMask;
            public float Speed;
            public float Range;
            public MovementDataSO PullData;
            public float PullStopDistance;
            public float ControlLockDuration;
        }

        [SerializeField] private Rigidbody rb;
        [SerializeField] private TrailRenderer trail;

        private Entity _owner;
        private Transform _anchor;
        private DamageData _damage;
        private Config _config;
        private Vector3 _spawnPosition;
        private bool _active;

        // 풀에 쓰일 maxSpeed/곡선은 그대로 두고 거리에 맞춰 duration만 늘리기 위한 런타임 복사본
        private MovementDataSO _pullData;

        public void Launch(Entity owner, Transform anchor, Vector3 direction, DamageData damage, Config config)
        {
            _owner = owner;
            _anchor = anchor;
            _damage = damage;
            _config = config;
            _spawnPosition = transform.position;
            _active = true;

            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
            transform.forward = direction;

            rb.linearVelocity = direction * config.Speed;
            trail?.Clear();
        }

        private void FixedUpdate()
        {
            if (_active && (transform.position - _spawnPosition).sqrMagnitude >= _config.Range * _config.Range)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_active || other.isTrigger)
                return;

            if ((_config.HitMask.value & (1 << other.gameObject.layer)) == 0)
                return;

            Entity target = other.GetComponentInParent<Entity>();
            if (target == null || target == _owner)
                return;

            _active = false;
            rb.linearVelocity = Vector3.zero;

            if (other.TryGetComponent(out IDamageable damageable) || target.TryGetComponent(out damageable))
            {
                damageable.ApplyDamage(new DamageContext
                {
                    DamageData = _damage,
                    HitPoint = other.ClosestPoint(transform.position),
                    HitNormal = -transform.forward,
                    Source = gameObject,
                    Attacker = _owner
                });
                _owner?.OnAttack?.Invoke(_owner, damageable);
            }

            PullAsync(target).Forget();
        }

        private async UniTaskVoid PullAsync(Entity target)
        {
            ISkillMovement movement = target.GetComponentInChildren<ISkillMovement>();
            Vector3 anchorPos = _anchor != null ? _anchor.position : transform.position;
            Vector3 toAnchor = anchorPos - target.transform.position;
            toAnchor.y = 0f;

            float distance = toAnchor.magnitude - _config.PullStopDistance;
            if (_config.PullData == null || movement == null || distance <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = toAnchor.normalized;
            float duration = distance / Mathf.Max(0.01f, _config.PullData.maxSpeed * AverageCurve(_config.PullData.moveCurve));

            _pullData = _pullData != null ? _pullData : ScriptableObject.CreateInstance<MovementDataSO>();
            _pullData.maxSpeed = _config.PullData.maxSpeed;
            _pullData.moveCurve = _config.PullData.moveCurve;
            _pullData.duration = duration;

            if (target is IStunable stunable)
                stunable.Stun(Mathf.Max(_config.ControlLockDuration, duration));

            bool canMove = movement.CanMove;
            movement.CanMove = false;
            movement.SetRotation(direction);
            movement.ApplyMovementData(direction, _pullData);

            await UniTask.WaitForSeconds(duration, cancellationToken: this.GetCancellationTokenOnDestroy());

            movement.CanMove = canMove;
            Destroy(gameObject);
        }

        // duration을 거리에 맞춰 선형으로 환산하기 위한 곡선 평균값(이동거리 = maxSpeed * duration * 평균)
        private static float AverageCurve(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
                return 1f;

            const int samples = 16;
            float sum = 0f;
            for (int i = 0; i < samples; i++)
                sum += curve.Evaluate((i + 0.5f) / samples);
            return Mathf.Max(0.01f, sum / samples);
        }

        private void OnDestroy()
        {
            if (_pullData != null)
                Destroy(_pullData);
        }
    }
}
