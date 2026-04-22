using System;
using System.Collections.Generic;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

[CreateAssetMenu(fileName = "ItemRecipeMap", menuName = "SO/ItemRecipeMap")]
public class CraftableItemListSO : ScriptableObject
{
    [Serializable]
    public class Recipe
    {
        public ItemDataSO item;
        public List<ItemDataSO> results = new();
    }
    
    public List<Recipe> recipes = new();
    private Dictionary<ItemDataSO, List<ItemDataSO>> _map;

    public void BuildDictionary()
    {
        _map = new Dictionary<ItemDataSO, List<ItemDataSO>>();

        foreach (var entry in recipes)
        {
            if (entry.item == null) continue;
            _map[entry.item] = entry.results;
        }
    }

    public List<ItemDataSO> GetRecipes(ItemDataSO item)
    {
        if (_map == null) BuildDictionary();
        return _map != null && _map.TryGetValue(item, out var list) ? list : null;
    }
}