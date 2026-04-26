using System;
using System.Collections.Generic;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

[Serializable]
public class Recipe
{
    public ItemDataSO item;
    public List<ItemDataSO> results = new();
}

[CreateAssetMenu(fileName = "ItemRecipeMap", menuName = "SO/ItemRecipeMap")]
public class CraftableItemListSO : ScriptableObject
{
    [HideInInspector] public List<Recipe> recipes = new();
    private Dictionary<ItemDataSO, List<ItemDataSO>> _itemMap;

    public void BuildDictionary()
    {
        _itemMap = new Dictionary<ItemDataSO, List<ItemDataSO>>();

        foreach (Recipe recipe in recipes)
        {
            if (recipe.item == null)
                continue;
            
            _itemMap[recipe.item] = recipe.results;
        }
    }

    public List<ItemDataSO> GetRecipes(ItemDataSO item)
    {
        if (_itemMap == null)
        {
            BuildDictionary();
        }
        
        return _itemMap != null && _itemMap.TryGetValue(item, 
            out List<ItemDataSO> itemList) ? itemList : null;
    }
}