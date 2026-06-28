using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.StatusEffectSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Areas;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Scripts.Entities;
using System;
using Ami.BroAudio;
using UnityEngine;

namespace Code.SkillSystem.Skills.Bombing
{
    public class BombingMissile : MonoBehaviour, IPoolable
    {
        [SerializeField] private LayerMask whatIsTarget;
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private PoolItemSO bombingItemSO;
        [SerializeField] private PoolItemSO bombEffectItemSO;
        [SerializeField] private PoolItemSO floorItemSO;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private BuffSO slowAndAdditionalDamageData;
        [SerializeField] private Rigidbody rigidbodyCompo;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SoundID bombSound;
        [SerializeField] private float hangDuration = 0.08f;
        [SerializeField] private float hoverAmplitude = 0.15f;
        [SerializeField] private float startFallSpeed = 2.5f;
        [SerializeField] private float fallAcceleration = 36f;
        [SerializeField] private float maxFallSpeed = 18f;
        [SerializeField] private float nearImpactAcceleration = 72f;
        [SerializeField] private float nearImpactMaxSpeed = 34f;
        [SerializeField] private float nearImpactDistance = 3.5f;
        [SerializeField] private float visualTilt = 18f;
        [SerializeField] private float visualSpinSpeed = 540f;
        [SerializeField] private float visualRotationLerp = 12f;
        [SerializeField] private float preImpactShakeForce = 2f;
        [SerializeField] private float impactShakeForce = 8f;

        [SerializeField] private AnimationCurve fallAccelerationCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [field: SerializeField] public bool CreateFloor { get; set; }
        [field: SerializeField] public bool SlowAndAdditionalDamage { get; set; }
        public PoolItemSO PoolItem => bombingItemSO;
        public GameObject GameObject => gameObject;

        private Pool _myPool;
        private DamageData _currentDamageData;
        private Entity _owner;
        private Vector3 _spawnPoint;
        private Vector3 _targetPoint;
        private Quaternion _visualBaseLocalRotation;
        private float _currentFallSpeed;
        private float _hangTimer;
        private bool _isFalling;
        private bool _hasExploded;
        private bool _didPlayPreImpactShake;

        public event Action OnPush;

        private void Awake()
        {
            _visualBaseLocalRotation = visualRoot.localRotation;
        }

        private void FixedUpdate()
        {
            if (_isFalling == false || _hasExploded)
                return;

            Vector3 currentPosition = rigidbodyCompo != null ? rigidbodyCompo.position : transform.position;

            if (_hangTimer > 0f)
            {
                _hangTimer -= Time.fixedDeltaTime;

                float hoverOffset = Mathf.Sin((1f - (_hangTimer / Mathf.Max(0.01f, hangDuration))) * Mathf.PI)
                                    * hoverAmplitude;
                currentPosition.y = _spawnPoint.y + hoverOffset;

                MoveMissile(currentPosition);
                UpdateVisual(0f);
                return;
            }

            float fallProgress = GetFallProgress(currentPosition.y);
            float curveValue = fallAccelerationCurve != null
                ? fallAccelerationCurve.Evaluate(fallProgress)
                : fallProgress;

            float targetFallSpeed = Mathf.Lerp(startFallSpeed, maxFallSpeed, curveValue);
            _currentFallSpeed =
                Mathf.MoveTowards(_currentFallSpeed, targetFallSpeed, fallAcceleration * Time.fixedDeltaTime);

            float remainingDistance = currentPosition.y - _targetPoint.y;
            if (remainingDistance <= nearImpactDistance)
            {
                _currentFallSpeed = Mathf.MoveTowards(
                    _currentFallSpeed,
                    nearImpactMaxSpeed,
                    nearImpactAcceleration * Time.fixedDeltaTime);

                if (_didPlayPreImpactShake == false)
                {
                    Bus.Raise(new CameraShakeEvent(currentPosition, Vector3.down, preImpactShakeForce));
                    _didPlayPreImpactShake = true;
                }
            }

            currentPosition.y =
                Mathf.Max(_targetPoint.y, currentPosition.y - (_currentFallSpeed * Time.fixedDeltaTime));
            MoveMissile(currentPosition);
            UpdateVisual(curveValue);

            if (currentPosition.y <= _targetPoint.y + 0.01f)
                Explode();
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void SetDamageData(DamageData damageData) => _currentDamageData = damageData;

        public void SetOwner(Entity owner)
        {
            _owner = owner;
            damageCaster.InitCaster(_owner);
        }

        public void BeginFall(Vector3 targetPoint)
        {
            _spawnPoint = transform.position;
            _targetPoint = new Vector3(_spawnPoint.x, targetPoint.y, _spawnPoint.z);
            _currentFallSpeed = startFallSpeed;
            _hangTimer = hangDuration;
            _isFalling = true;
            _hasExploded = false;
            _didPlayPreImpactShake = false;

            if (rigidbodyCompo != null)
            {
                rigidbodyCompo.useGravity = false;
                rigidbodyCompo.isKinematic = true;
                rigidbodyCompo.linearVelocity = Vector3.zero;
                rigidbodyCompo.angularVelocity = Vector3.zero;
                rigidbodyCompo.position = _spawnPoint;
                rigidbodyCompo.rotation = Quaternion.identity;
            }
            else
            {
                transform.position = _spawnPoint;
                transform.rotation = Quaternion.identity;
            }

            if (visualRoot != null)
                visualRoot.localRotation = _visualBaseLocalRotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isFalling == false || _hasExploded)
                return;

            Explode();
        }

        private void Explode()
        {
            if (_hasExploded)
                return;

            _hasExploded = true;
            _isFalling = false;

            if (rigidbodyCompo != null)
            {
                rigidbodyCompo.linearVelocity = Vector3.zero;
                rigidbodyCompo.angularVelocity = Vector3.zero;
            }

            damageCaster.CastDamage(_currentDamageData, damageCaster.transform.position, Vector3.down, null);

            PoolingEffect poolingEffect = poolManagerSO.Pop(bombEffectItemSO) as PoolingEffect;
            poolingEffect.PlayVFX(damageCaster.transform.position, Quaternion.identity);

            if (SlowAndAdditionalDamage)
                ApplySlowAndAdditionalDamage();

            if (CreateFloor)
            {
                DealingArea floor = poolManagerSO.Pop(floorItemSO) as DealingArea;
                floor.Init(_owner, damageCaster.transform.position);
            }
            
            Bus.Raise(new CameraShakeEvent(damageCaster.transform.position, Vector3.down, impactShakeForce));
            BroAudio.Play(bombSound, transform.position);
            
            OnPush?.Invoke();
            _myPool.Push(this);
        }

        public void ApplySlowAndAdditionalDamage()
        {
            Collider[] targets = new Collider[10];

            Vector3 overlapPos = transform.position;

            Physics.Raycast(overlapPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, whatIsTarget);

            overlapPos.y = hit.point.y;

            int count = Physics.OverlapSphereNonAlloc(overlapPos, 4, targets, whatIsTarget);

            for (int i = 0; i < count; i++)
            {
                ComponentContainer compoContainer = targets[i].gameObject.GetComponent<ComponentContainer>();
                EntityStatusEffect entityStatusEffect = compoContainer.Get<EntityStatusEffect>();

                entityStatusEffect.AddStatusEffect(slowAndAdditionalDamageData.GetStatusEffectInfo());
            }
        }

        public void ResetItem()
        {
            _isFalling = false;
            _hasExploded = false;
            _didPlayPreImpactShake = false;
            _currentFallSpeed = 0f;
            _hangTimer = 0f;
            OnPush = null;

            if (rigidbodyCompo != null)
            {
                rigidbodyCompo.useGravity = false;
                rigidbodyCompo.isKinematic = true;
                rigidbodyCompo.linearVelocity = Vector3.zero;
                rigidbodyCompo.angularVelocity = Vector3.zero;
            }

            if (visualRoot != null)
                visualRoot.localRotation = _visualBaseLocalRotation;
        }

        private void MoveMissile(Vector3 position)
        {
            if (rigidbodyCompo != null)
                rigidbodyCompo.MovePosition(position);
            else
                transform.position = position;
        }

        private void UpdateVisual(float fallRatio)
        {
            if (visualRoot == null)
                return;

            float tiltAngle = Mathf.Lerp(0f, visualTilt, fallRatio);
            float spinAngle = Time.fixedTime * visualSpinSpeed;
            Quaternion targetRotation = _visualBaseLocalRotation * Quaternion.Euler(tiltAngle, spinAngle, 0f);

            visualRoot.localRotation = Quaternion.Slerp(
                visualRoot.localRotation,
                targetRotation,
                visualRotationLerp * Time.fixedDeltaTime);
        }

        private float GetFallProgress(float currentHeight)
        {
            float totalDistance = Mathf.Max(0.01f, _spawnPoint.y - _targetPoint.y);
            float travelledDistance = _spawnPoint.y - currentHeight;
            return Mathf.Clamp01(travelledDistance / totalDistance);
        }
    }
}