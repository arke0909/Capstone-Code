using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using SHS.Scripts.Combats.Events;
using UnityEngine;

namespace Code.SkillSystem.Skills.StackBomb
{
    public class StackBomb : MonoBehaviour, IPoolable
    {
        [SerializeField] private float delayToBomb = 5f;
        [SerializeField] private float defaultDamage = 10f;
        [SerializeField] private float stackPerDamage = 3f;
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private int defPierceLevel = 1;
        [SerializeField] private DamageType damageType = DamageType.MAGIC;
        [SerializeField] private float moveDuration = 0.35f;
        [SerializeField] private float heightPerDistance = 0.1f;
        [SerializeField] private float minArcHeight = 0.2f, maxArcHeight = 2f;

        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;

        private int _hitStack;
        private Pool _myPool;
        private float _arcHeight;
        private Entity _owner;

        public async UniTask Init(Entity owner, Transform targetTrm)
        {
            _owner = owner;

            if (targetTrm == null || targetTrm.TryGetComponent(out Entity target) == false)
            {
                ReturnToPool();
                return;
            }

            bool hasArrived = await MoveBomb(transform.position, target.HitTransform.position);
            if (!hasArrived)
            {
                ReturnToPool();
                return;
            }

            target.OnHitEvent.AddListener(HandleTargetOnHit);
            await UniTask.WaitForSeconds(delayToBomb);

            if (target != null)
                target.OnHitEvent.RemoveListener(HandleTargetOnHit);

            if (target == null || target.IsDead)
            {
                ReturnToPool();
                return;
            }

            transform.position = target.HitTransform.position;
            TryDealDamage(target);
            ReturnToPool();
        }

        private async UniTask<bool> MoveBomb(Vector3 startPoint, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(startPoint, targetPosition);
            _arcHeight = Mathf.Clamp(distance * heightPerDistance, minArcHeight, maxArcHeight);

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / moveDuration);
                transform.position = EvaluateArc(startPoint, targetPosition, t);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            transform.position = targetPosition;
            return true;
        }

        private Vector3 EvaluateArc(Vector3 startPoint, Vector3 targetPoint, float t)
        {
            Vector3 pos = Vector3.Lerp(startPoint, targetPoint, t);

            float arc = 4f * t * (1f - t);
            pos.y += _arcHeight * arc;

            return pos;
        }

        private void TryDealDamage(Entity target)
        {
            IDamageable damageable = target.GetSubclassCompo<IDamageable>();
            if (damageable == null)
                damageable = target.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            Vector3 hitPoint = target.HitTransform.position;
            Vector3 hitNormal = (_owner != null ? _owner.transform.position : transform.position) - hitPoint;
            if (hitNormal.sqrMagnitude <= 0.0001f)
                hitNormal = -target.transform.forward;
            else
                hitNormal.Normalize();

            DamageContext context = BuildDamageContext(target.HitTransform, hitPoint, hitNormal);
            damageable.ApplyDamage(context);

            if (_owner != null)
            {
                _owner.LocalEventBus?.Raise(new AttackHitEvent(damageable, context));
                _owner.OnAttack?.Invoke(_owner, damageable);
            }
        }

        private DamageContext BuildDamageContext(Transform hitTransform, Vector3 hitPoint, Vector3 hitNormal)
        {
            float finalDamage = Mathf.Max(0f, defaultDamage + (_hitStack * stackPerDamage));
            float finalDamageMultiplier = damageMultiplier;

            if (_owner != null && _owner.OnDamageCalc != null)
            {
                foreach (Entity.OnDamageCalcDelegate damageCalc in _owner.OnDamageCalc.GetInvocationList())
                    finalDamageMultiplier += damageCalc(_owner, hitTransform);
            }

            DamageCalcCompo damageCalcCompo = _owner != null ? _owner.Get<DamageCalcCompo>() : null;
            DamageData damageData = damageCalcCompo != null
                ? damageCalcCompo.CalculateDamage(finalDamage, finalDamageMultiplier, defPierceLevel, damageType)
                : new DamageData
                {
                    damage = finalDamage * finalDamageMultiplier,
                    defPierceLevel = defPierceLevel,
                    damageType = damageType
                };

            return new DamageContext
            {
                DamageData = damageData,
                HitPoint = hitPoint,
                HitNormal = hitNormal,
                Source = gameObject,
                Attacker = _owner
            };
        }

        private void HandleTargetOnHit()
        {
            _hitStack++;
        }

        private void ReturnToPool()
        {
            if (_myPool != null)
                _myPool.Push(this);
            else
                gameObject.SetActive(false);
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            _hitStack = 0;
            _arcHeight = 0f;
            _owner = null;
        }
    }
}
