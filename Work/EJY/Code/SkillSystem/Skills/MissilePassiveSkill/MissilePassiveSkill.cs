using System.Collections.Generic;
using Ami.BroAudio;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SkillSystem.Skills.MissilePassiveSkill
{
    public class MissilePassiveSkill : PassiveSkill
    {
        [SerializeField] private Transform firePosTrm;
        [SerializeField] private int hitCntToFireMissile = 5;
        [SerializeField] private int shotMissile = 1;
        [SerializeField] private int additionalMissile = 2;
        [SerializeField] private int downHitCnt = 2;
        [SerializeField] private PoolItemSO missilePoolItem;
        [SerializeField] private bool isDmgRangIncrease;
        [SerializeField] private bool isInduction;
        [SerializeField] private float additionalDmgRange = 2.5f;
        [SerializeField] private float launchRiseHeight = 2f;
        [SerializeField] private float randomSpawnRangeX = 1.5f;
        [SerializeField] private float randomSpawnRangeZ = 1.5f;
        [SerializeField] private float spawnHeightOffset = 0f;
        [SerializeField] private float minSpawnSpacing = 0.5f;
        
        [Inject] private PoolManagerMono _poolManager;
        private int _currentHitCnt = 0;

        public override void EnableSkill()
        {
            base.EnableSkill();
            Debug.Log("미사일 작동");
            _owner.OnHit += HandleOnHit;
        }

        public override void DisableSkill()
        {
            _owner.OnHit -= HandleOnHit;
            base.DisableSkill();
        }

        private void UpgradeDmgRange() => isDmgRangIncrease = true;
        private void RollbackDmgRange() => isDmgRangIncrease = false;

        private void UpgradeMultiShot()
        {
            isInduction = true;
            hitCntToFireMissile -= downHitCnt;
            shotMissile += additionalMissile;
        }

        private void RollbackMultiShot()
        {
            isInduction = false;
            hitCntToFireMissile += downHitCnt;
            shotMissile -= additionalMissile;
        }

        private void HandleOnHit(Entity dealer, IDamageable target)
        {
            if (target is not MonoBehaviour targetMono)
            {
                Debug.Log("target is not MonoBehaviour");
                return;
            }

            _currentHitCnt++;

            if (_currentHitCnt >= hitCntToFireMissile)
            {
                Transform targetRootTrm = targetMono.transform.root;
                IHitTransform hitTransform = targetRootTrm.gameObject.GetComponent<IHitTransform>();

                if (hitTransform == null)
                    return;

                List<Vector3> middlePoints = GenerateMiddlePoints();

                for (int i = 0; i < shotMissile; ++i)
                {
                    var missile = _poolManager.Pop<Missile>(missilePoolItem);
                    missile.InitMissile(_owner, hitTransform.HitTransform, firePosTrm.position, isInduction, GenerateLaunchOffset(), middlePoints[i]);
                    if (isDmgRangIncrease)
                        missile.SetDmgRange(additionalDmgRange);
                }

                _currentHitCnt = 0;
                
                OnSkillInvoked?.Invoke();
            }
        }

        private Vector3 GenerateLaunchOffset()
        {
            return Vector3.up * launchRiseHeight;
        }

        private List<Vector3> GenerateMiddlePoints()
        {
            List<Vector3> positions = new List<Vector3>(shotMissile);
            GetLaunchBasis(out Vector3 forward, out Vector3 right);
            Vector3 center = firePosTrm.position + GenerateLaunchOffset() + Vector3.up * spawnHeightOffset;
            float minSpacingSqr = minSpawnSpacing * minSpawnSpacing;

            for (int i = 0; i < shotMissile; i++)
            {
                Vector3 candidate = center;
                bool foundSpacedPoint = false;

                for (int attempt = 0; attempt < 8; attempt++)
                {
                    float lateralOffset = Random.Range(-randomSpawnRangeX, randomSpawnRangeX);
                    float forwardOffset = Random.Range(0f, randomSpawnRangeZ);
                    candidate = center + (right * lateralOffset) + (forward * forwardOffset);

                    if (IsFarEnough(candidate, positions, minSpacingSqr))
                    {
                        foundSpacedPoint = true;
                        break;
                    }
                }

                if (foundSpacedPoint == false && positions.Count > 0)
                {
                    float t = shotMissile <= 1 ? 0.5f : i / (float)(shotMissile - 1);
                    float lateralOffset = Mathf.Lerp(-randomSpawnRangeX, randomSpawnRangeX, t);
                    candidate = center + (right * lateralOffset) + (forward * randomSpawnRangeZ);
                }

                positions.Add(candidate);
            }

            return positions;
        }

        private void GetLaunchBasis(out Vector3 forward, out Vector3 right)
        {
            Transform basisTrm = _owner != null ? _owner.transform : firePosTrm;

            forward = Vector3.ProjectOnPlane(basisTrm.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;
            else
                forward.Normalize();

            right = Vector3.ProjectOnPlane(basisTrm.right, Vector3.up);
            if (right.sqrMagnitude < 0.0001f)
                right = Vector3.Cross(Vector3.up, forward).normalized;
            else
                right.Normalize();
        }

        private bool IsFarEnough(Vector3 candidate, List<Vector3> existingPositions, float minSpacingSqr)
        {
            for (int i = 0; i < existingPositions.Count; i++)
            {
                if ((existingPositions[i] - candidate).sqrMagnitude < minSpacingSqr)
                    return false;
            }

            return true;
        }
    }
}
