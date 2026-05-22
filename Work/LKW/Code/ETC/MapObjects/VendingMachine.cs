using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using Scripts.Entities;
using Scripts.GameSystem;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;
using Random = UnityEngine.Random;

namespace Code.ETC.MapObjects
{
    public class VendingMachine : InteractableStructure
    {
        private const string EmissionKeyword = "_EMISSION";
        private const string EmissionColorKeyword = "_EmissionColor";
        private const float EmissionIntensity = 10.0f;
        private static readonly int EmissionColorID = Shader.PropertyToID(EmissionColorKeyword);
        private static readonly Color BaseEmissionColor = Color.white * EmissionIntensity;
        
        private static readonly Dictionary<Rarity, Color> RarityColors = new()
        {
            { Rarity.Common, Color.white },
            { Rarity.Rare,   Color.cyan },
            { Rarity.Epic,   new Color(0.6f, 0f, 1f) }
        };

        [Header("Reference")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Transform discardPoint;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private ItemDropper itemDropper;
        [SerializeField] private SoundID dropSoundID;

        [Header("Settings")]
        [SerializeField] private int minDropCount = 1;
        [SerializeField] private int maxDropCount = 5;
        [SerializeField] private float offDuration = 1.6f;
        [SerializeField] private float discardRange = 0.5f;
        [SerializeField] private float interactCooldown = 1f;

        private int _dropCount = 0;
        private bool _isOff = false;
        private float _lastInteractTime = float.NegativeInfinity;
        private Material _material;

        protected override void Awake()
        {
            base.Awake();
            _material = meshRenderer.material;
        }

        protected override void Start()
        {
            base.Start();
            Init();
        }

        private void Init()
        {
            _material.EnableKeyword(EmissionKeyword);
            _material.SetColor(EmissionColorID, BaseEmissionColor);
            _isOff = false;
            _dropCount = 0;
        }

        private bool CanInteract()
            => !_isOff
            && Time.time - _lastInteractTime >= interactCooldown
            && _dropCount < maxDropCount;

        public override void Interact(Entity interactor)
        {
            if (!CanInteract()) return;

            if (_dropCount > 1 && Random.value < 0.5f)
            {
                _dropCount = maxDropCount;
                StartCoroutine(MachineOffCoroutine(offDuration));
                return;
            }

            SpawnItem();
            _dropCount++;
            _lastInteractTime = Time.time;
        }

        private void SpawnItem()
        {
            Vector3 to = discardPoint.position;
            to.x += Random.Range(-discardRange, discardRange);
            to.z += Random.Range(-discardRange, discardRange);

            PreviewItem targetItem = itemDropper.Drop(spawnPoint.position, to);
            BroAudio.Play(dropSoundID, transform.position);

            if (RarityColors.TryGetValue(targetItem.Item.ItemData.rarity, out var color))
                StartCoroutine(RarityFlashCoroutine(color));
        }

        private IEnumerator RarityFlashCoroutine(Color rarityColor)
        {
            Color flashColor = rarityColor * EmissionIntensity;
            float flashDuration = 1f;

            _material.SetColor(EmissionColorID, flashColor);
            yield return new WaitForSeconds(flashDuration);
            _material.SetColor(EmissionColorID, BaseEmissionColor);
            yield return new WaitForSeconds(flashDuration);

            _material.SetColor(EmissionColorID, BaseEmissionColor);
        }

        private IEnumerator MachineOffCoroutine(float duration)
        {
            _isOff = true;
            Color startColor = _material.GetColor(EmissionColorID);
            Color endColor = Color.black;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _material.SetColor(EmissionColorID, Color.Lerp(startColor, endColor, elapsed / duration));
                yield return null;
            }

            _material.SetColor(EmissionColorID, endColor);
            _material.DisableKeyword(EmissionKeyword);
        }
    }
}