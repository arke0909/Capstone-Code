using System;
using System.Collections.Generic;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;
using Random = UnityEngine.Random;

namespace Code.ETC.MapObjects
{
    [Serializable]
    public class RarityWeight
    {
        public Rarity rarity;
        public float weight;
    }

    [Serializable]
    public class ItemTypeDropConfig
    {
        public ItemType type;
        public List<RarityWeight> rarityWeights;
    }
    
public class ItemDropper : MonoBehaviour
{
    [Inject]
    private PoolManagerMono _poolManagerMono;

    [SerializeField] private ItemDataBaseSO itemDB;
    [SerializeField] private PoolItemSO previewItemPool;
    [SerializeField] private List<ItemTypeDropConfig> dropConfigs;

    [Header("Drop Settings")]
    [SerializeField] private float arcHeight = 1f;
    [SerializeField] private float dropDuration = 0.4f;

    public PreviewItem Drop(Vector3 from, Vector3 to)
    {
        if (dropConfigs == null || dropConfigs.Count == 0) return null;

        var config = dropConfigs[Random.Range(0, dropConfigs.Count)];
        if (config.rarityWeights == null || config.rarityWeights.Count == 0) return null;

        var rarity = GetWeightedRarity(config.rarityWeights);
        var items = itemDB.GetRandomItems(config.type, rarity, 1);
        if (items == null || items.Count == 0) return null;

        var createData = items[0].CreateItem();
        var item = _poolManagerMono.Pop<PreviewItem>(previewItemPool);
        if (item == null) return null;

        item.Discard(from, createData.Item, createData.Stack);

        Sequence seq = DOTween.Sequence();
        seq.Append(item.transform.DOMoveX(to.x, dropDuration).SetEase(Ease.Linear));
        seq.Join(item.transform.DOMoveZ(to.z, dropDuration).SetEase(Ease.Linear));
        seq.Join(item.transform.DOMoveY(from.y + arcHeight, dropDuration * 0.4f).SetEase(Ease.OutQuad));
        seq.Insert(dropDuration * 0.4f, item.transform.DOMoveY(to.y, dropDuration * 0.6f).SetEase(Ease.InQuad));
        seq.OnComplete(() => item.transform.DOPunchScale(Vector3.one * 0.15f, 0.15f, 1, 0f));

        return item;
    }

    private Rarity GetWeightedRarity(List<RarityWeight> rarityWeights)
    {
        float total = 0f;
        foreach (var rw in rarityWeights)
            total += rw.weight;

        float roll = Random.Range(0f, total);
        float current = 0f;

        foreach (var rw in rarityWeights)
        {
            current += rw.weight;
            if (roll <= current) return rw.rarity;
        }

        return rarityWeights[rarityWeights.Count - 1].rarity;
    }
}
}