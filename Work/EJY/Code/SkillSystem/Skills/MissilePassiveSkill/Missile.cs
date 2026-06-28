using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Scripts.Entities;
using UnityEngine;

namespace Code.SkillSystem.Skills.MissilePassiveSkill
{
    public class Missile : MonoBehaviour, IPoolable
    {
        [SerializeField] private OverlapDamageCaster overlapDamageCaster;
        [SerializeField] private PoolItemSO missilePoolItem;
        [SerializeField] private PoolItemSO missileExplosionPoolItem;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private float missileSpeed = 8f;
        [SerializeField] private float searchRadius = 10f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private float targetImpactDistance = 1.1f;
        [SerializeField] private float impactArmDelay = 0.05f;
        [SerializeField] private float camShakeForce = 12f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private MeshRenderer meshRenderer;
        
        [SerializeField] private SoundID jetSoundID;
        [SerializeField] private SoundID explosionSoundID;

        public PoolItemSO PoolItem => missilePoolItem;
        public GameObject GameObject => gameObject;

        private Pool _myPool;
        private Entity _owner;
        private DamageCalcCompo _dmgCalcCompo;
        private Rigidbody _rigidbody;

        private bool _isDead;
        private bool _hasFixedTargetPoint;
        private bool _isCurveActive;
        private bool _isInitialized;

        private Vector3 _lastMoveDir;
        private Vector3 _fixedTargetPoint;
        private Vector3 _curveStartPoint;
        private Vector3 _curveControlPoint;
        private Vector3 _curveEndPoint;
        private float _curveProgress;
        private float _curveLength;
        private float _impactArmTimer;
        private float _damage;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            if (particle != null)
            {
                var main = particle.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;

                var velocityOverLifetime = particle.velocityOverLifetime;
                velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            }
        }

        public void InitMissile(Entity owner, Transform target, Vector3 position, Vector3 launchOffset, Vector3 initialCurveControlPoint, float damage)
        {
            _owner = owner;
            _isDead = false;
            _isInitialized = false;
            _impactArmTimer = impactArmDelay;
            _fixedTargetPoint = target != null
                ? target.position
                : position + ((_owner != null ? _owner.transform.forward : transform.forward) * searchRadius);
            _hasFixedTargetPoint = true;

            transform.position = position;
            _rigidbody.position = position;
            _rigidbody.rotation = Quaternion.identity;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            _damage = damage;

            particle.Clear();
            particle.gameObject.SetActive(true);

            _dmgCalcCompo = _owner.Get<DamageCalcCompo>();
            overlapDamageCaster.InitCaster(owner);

            Vector3 launchDir = _fixedTargetPoint - position;
            if (launchDir.sqrMagnitude < 0.0001f)
                launchDir = _owner != null ? _owner.transform.forward : transform.forward;

            _lastMoveDir = launchDir.normalized;
            _rigidbody.rotation = Quaternion.LookRotation(_lastMoveDir);

            Vector3 controlPoint = initialCurveControlPoint;
            if ((controlPoint - position).sqrMagnitude < 0.0001f)
                controlPoint = position + launchOffset;

            BuildBezier(position, _fixedTargetPoint, controlPoint);

            if (jetSoundID.IsValid())
                BroAudio.Play(jetSoundID, transform.position);

            _rigidbody.linearVelocity = _lastMoveDir * missileSpeed;
            _isInitialized = true;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        private void FixedUpdate()
        {
            if (_isDead || _isInitialized == false)
                return;

            if (_impactArmTimer > 0f)
                _impactArmTimer -= Time.fixedDeltaTime;

            if (TryImpactTrackedTarget())
                return;

            if (_isCurveActive)
            {
                MoveAlongBezier();
                return;
            }

            if (TryGetTargetPoint(out Vector3 targetPoint))
            {
                MoveTowardPoint(targetPoint);
                return;
            }

            MoveForward();
        }

        private bool TryGetTargetPoint(out Vector3 targetPoint)
        {
            if (_hasFixedTargetPoint)
            {
                targetPoint = _fixedTargetPoint;
                return true;
            }

            targetPoint = Vector3.zero;
            return false;
        }

        private void BuildBezier(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint)
        {
            _curveStartPoint = startPoint;
            _curveEndPoint = endPoint;
            _curveProgress = 0f;
            _isCurveActive = true;
            _curveControlPoint = controlPoint;
            _curveLength = EstimateQuadraticBezierLength(_curveStartPoint, _curveControlPoint, _curveEndPoint);
        }

       private void MoveAlongBezier()
       {
           if (_curveLength < 0.0001f)
           {
               _isCurveActive = false;
               return;
           }
       
           _curveProgress += (missileSpeed * Time.fixedDeltaTime) / _curveLength;
           _curveProgress = Mathf.Clamp01(_curveProgress);
       
           Vector3 point = EvaluateQuadraticBezier(
               _curveStartPoint,
               _curveControlPoint,
               _curveEndPoint,
               _curveProgress
           );
       
           Vector3 tangent = EvaluateQuadraticBezierTangent(
               _curveStartPoint,
               _curveControlPoint,
               _curveEndPoint,
               _curveProgress
           );
       
           if (tangent.sqrMagnitude < 0.0001f)
               return;
       
           tangent.Normalize();
       
           _rigidbody.MovePosition(point);
           _rigidbody.linearVelocity = tangent * missileSpeed;
       
           Quaternion rot = Quaternion.LookRotation(tangent);
           _rigidbody.MoveRotation(rot);
       
           _lastMoveDir = tangent;

           if (_curveProgress >= 1f)
               _isCurveActive = false;
       }

        private void MoveTowardPoint(Vector3 point)
        {
            Vector3 desiredDir = point - _rigidbody.position;
            if (desiredDir.sqrMagnitude < 0.0001f)
                return;

            ApplyVelocity(desiredDir.normalized);
        }

        private void MoveForward()
        {
            ApplyVelocity(GetCurrentDirection());
        }

        private void ApplyVelocity(Vector3 desiredDir)
        {
            if (desiredDir.sqrMagnitude < 0.0001f)
                return;

            desiredDir.Normalize();
            _rigidbody.linearVelocity = desiredDir * missileSpeed;

            if (_isCurveActive)
            {
                Quaternion rot = Quaternion.LookRotation(desiredDir);
                _rigidbody.MoveRotation(rot);
                _lastMoveDir = desiredDir;
            }
            else
            {
                SmoothRotate(desiredDir, Time.fixedDeltaTime);
            }
        }

        private Vector3 GetCurrentDirection()
        {
            if (_rigidbody.linearVelocity.sqrMagnitude > 0.0001f)
                return _rigidbody.linearVelocity.normalized;

            if (_lastMoveDir.sqrMagnitude > 0.0001f)
                return _lastMoveDir.normalized;

            if (_owner != null)
                return _owner.transform.forward;

            return transform.forward;
        }

        private bool TryImpactTrackedTarget()
        {
            if (_impactArmTimer > 0f)
                return false;

            if (TryGetTargetPoint(out Vector3 targetPoint) == false)
                return false;

            float impactDistance = Mathf.Max(targetImpactDistance, missileSpeed * Time.fixedDeltaTime * 1.5f);
            if ((targetPoint - _rigidbody.position).sqrMagnitude > impactDistance * impactDistance)
                return false;

            _rigidbody.position = targetPoint;
            transform.position = targetPoint;
            TriggerImpact();
            return true;
        }

        private Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0
                + 2f * oneMinusT * t * p1
                + t * t * p2;
        }

        private Vector3 EvaluateQuadraticBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
        }

        private float EstimateQuadraticBezierLength(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            const int sampleCount = 12;
            float length = 0f;
            Vector3 previousPoint = p0;

            for (int i = 1; i <= sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                Vector3 point = EvaluateQuadraticBezier(p0, p1, p2, t);
                length += Vector3.Distance(previousPoint, point);
                previousPoint = point;
            }

            return length;
        }

        private void SmoothRotate(Vector3 dir, float deltaTime)
        {
            if (dir.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            Quaternion newRot = Quaternion.RotateTowards(_rigidbody.rotation, targetRot, rotationSpeed * deltaTime);

            _rigidbody.MoveRotation(newRot);
            _lastMoveDir = newRot * Vector3.forward;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isDead || _isInitialized == false || _impactArmTimer > 0f)
                return;

            if (_owner != null && other.transform.root == _owner.transform)
                return;

            TriggerImpact();
        }

        private void TriggerImpact()
        {
            if (_isDead)
                return;

            if (jetSoundID.IsValid())
                BroAudio.Stop(jetSoundID);
            
            if (explosionSoundID.IsValid())
                BroAudio.Play(explosionSoundID);
            
            _isDead = true;
            _isCurveActive = false;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            HandleImpactAsync().Forget();
        }

        private async UniTaskVoid HandleImpactAsync()
        {
            particle.Stop();

            float remaining = 0f;
            int count = particle.particleCount;
            if (count > 0)
            {
                ParticleSystem.Particle[] particleArr = new ParticleSystem.Particle[count];
                int aliveCount = particle.GetParticles(particleArr);

                if (aliveCount > 0)
                {
                    ParticleSystem.Particle lastAlive = particleArr[0];
                    for (int i = 1; i < aliveCount; i++)
                    {
                        if (particleArr[i].remainingLifetime > lastAlive.remainingLifetime)
                            lastAlive = particleArr[i];
                    }

                    remaining = lastAlive.remainingLifetime;
                }
            }

            var data = _dmgCalcCompo.CalculateDamage(_damage, 1, 1, DamageType.RANGE);
            overlapDamageCaster.CastDamage(data, _rigidbody.position, _lastMoveDir, null);

            if (missileExplosionPoolItem != null)
            {
                PoolingEffect effect = poolManager.Pop(missileExplosionPoolItem) as PoolingEffect;
                effect?.PlayVFX(_rigidbody.position, Quaternion.identity);
            }

            meshRenderer.enabled = false;

            Bus.Raise(new CameraShakeEvent(transform.position, Vector3.down, camShakeForce));
            await UniTask.WaitForSeconds(remaining + 0.2f);

            particle.gameObject.SetActive(false);
            _myPool.Push(this);
        }

        public void ResetItem()
        {
            meshRenderer.enabled = true;

            _isDead = false;
            _hasFixedTargetPoint = false;
            _isCurveActive = false;
            _isInitialized = false;
            _owner = null;
            _dmgCalcCompo = null;
            _impactArmTimer = 0f;
            _fixedTargetPoint = Vector3.zero;
            _curveStartPoint = Vector3.zero;
            _curveControlPoint = Vector3.zero;
            _curveEndPoint = Vector3.zero;
            _curveProgress = 0f;
            _curveLength = 0f;
            _lastMoveDir = Vector3.zero;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            particle.Play();
            overlapDamageCaster.ResetRadius();
        }

        public void SetDmgRange(float radius)
        {
            overlapDamageCaster.SetRadius(overlapDamageCaster.CastRadius + radius);
        }
    }
}
