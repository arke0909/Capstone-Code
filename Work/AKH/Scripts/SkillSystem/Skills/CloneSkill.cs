using Code.EnemySpawn;
using Code.SHS.Entities.Enemies;
using Cysharp.Threading.Tasks;
using Scripts.SkillSystem.Manage;
using System;
using System.Collections.Generic;
using Ami.BroAudio;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    [Serializable]
    public struct CloneInfo
    {
        public EnemySO shadowSO;
        public Transform transform;
    }
    public class CloneSkill : ActiveSkill
    {
        [SerializeField] private SoundID cloneSound;
        [SerializeField] private GameObject smokeEffectPrefab;
        [SerializeField] private CloneInfo[] cloneInfos;
        [SerializeField] private float hideDuration = 0.75f;
        [SerializeField] private float smokeDuration = 0.75f;
        [SerializeField, Min(0f)] private float allShadowsDeadCooldown = 5f;

        private readonly List<Enemy> _activeShadows = new();
        private int _generation;

        public override async void StartSkill()
        {
            base.StartSkill();

            int generation = ++_generation;
            DespawnActiveShadows();

            _owner.gameObject.SetActive(false);
            await UniTask.WaitForSeconds(hideDuration);

            if (this == null || _owner == null || generation != _generation)
                return;

            _owner.gameObject.SetActive(true);
            GenerateSmoke(_owner.transform);
            SpawnShadows();
            BroAudio.Play(cloneSound, _owner.transform.position);
        }

        private void SpawnShadows()
        {
            foreach (CloneInfo item in cloneInfos)
            {
                if (item.transform == null)
                    continue;

                Enemy shadow = EnemySpawnUtility.SpawnEnemy(item.shadowSO, item.transform.position, Quaternion.identity);
                if (shadow == null)
                    continue;

                shadow.OnDeadEvent.AddListener(HandleShadowDead);
                _activeShadows.Add(shadow);
            }
        }

        private void HandleShadowDead()
        {
            PruneDeadShadows();

            if (_activeShadows.Count > 0)
                return;

            if (TryGetSkillSocket(out ActiveSkillSocket skillSocket) && skillSocket.IsCoolingDown)
                skillSocket.SetCooldown(allShadowsDeadCooldown);
        }

        private void PruneDeadShadows()
        {
            for (int i = _activeShadows.Count - 1; i >= 0; i--)
            {
                Enemy shadow = _activeShadows[i];

                if (shadow != null && !shadow.IsDead)
                    continue;

                if (shadow != null)
                    shadow.OnDeadEvent.RemoveListener(HandleShadowDead);

                _activeShadows.RemoveAt(i);
            }
        }

        private void DespawnActiveShadows()
        {
            for (int i = _activeShadows.Count - 1; i >= 0; i--)
            {
                Enemy shadow = _activeShadows[i];
                if (shadow == null)
                    continue;

                shadow.OnDeadEvent.RemoveListener(HandleShadowDead);
                shadow.ReleaseToPool();
            }

            _activeShadows.Clear();
        }

        private bool TryGetSkillSocket(out ActiveSkillSocket skillSocket)
        {
            skillSocket = null;

            if (_container == null || !_container.TryGetComponent(out ActiveSkillComponent skillComponent))
                return false;

            skillSocket = skillComponent.GetSocket(this) as ActiveSkillSocket;
            return skillSocket != null;
        }

        private void GenerateSmoke(Transform item)
        {
            GameObject smoke = Instantiate(smokeEffectPrefab, item.position, Quaternion.identity);
            Destroy(smoke, smokeDuration);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _activeShadows.Count; i++)
            {
                Enemy shadow = _activeShadows[i];
                if (shadow != null)
                    shadow.OnDeadEvent.RemoveListener(HandleShadowDead);
            }
        }
    }
}
