using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Code.SkillSystem.Skills.EnergyBalls
{
    public class RemnantFloor : MonoBehaviour, IPoolable
    {
        private static readonly int RatioHash = Shader.PropertyToID("_Ratio");

        [SerializeField] private PoolItemSO floorItem;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private ParticleSystem emberParticle;
        [SerializeField] private ParticleSystem explosionParticle;

        [SerializeField] private float delayToExplosion = 1.2f;
        [SerializeField] private float disappearDuration = 2f;

        private DamageCalcCompo _damageCalc;
        private Pool _myPool;
        private Material _floorMaterial;
        private ParticleSystem.EmissionModule _emission;

        private bool _isPreheating;
        private float _currentTime = 0f;

        public PoolItemSO PoolItem => floorItem;
        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _floorMaterial = GetComponentInChildren<Renderer>().material;
            _emission = emberParticle.emission;
        }

        public void Init(Entity owner, Vector3 position)
        {
            damageCaster.InitCaster(owner);
            transform.position = position;

            _damageCalc = owner.Get<DamageCalcCompo>();

            _isPreheating = true;
        }

        private void Update()
        {
            if (_isPreheating)
            {
                _currentTime += Time.deltaTime;

                float ratio = _currentTime / delayToExplosion;

                _floorMaterial.SetFloat(RatioHash, ratio);
                _emission.rateOverTime = ratio * 10f;

                if (_currentTime >= delayToExplosion)
                {
                    DamageData damageData =
                        _damageCalc.CalculateDamage(7, 1, 1, DamageType.MAGIC);

                    Explosion(damageData).Forget();
                }
            }
        }

        private async UniTaskVoid Explosion(DamageData damageData)
        {
            _isPreheating = false;

            damageCaster.CastDamage(damageData, transform.position, transform.up, null);

            explosionParticle.Play();

            float elapsed = 0f;

            while (elapsed < disappearDuration)
            {
                elapsed += Time.deltaTime;

                float ratio = 1f - (elapsed / disappearDuration);

                _floorMaterial.SetFloat(RatioHash, ratio);
                _emission.rateOverTime = ratio * 10f;

                await UniTask.Yield();
            }

            _floorMaterial.SetFloat(RatioHash, 0f);

            _myPool.Push(this);
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            _floorMaterial.SetFloat(RatioHash, 0f);

            _currentTime = 0f;
            _isPreheating = false;
        }
    }
}