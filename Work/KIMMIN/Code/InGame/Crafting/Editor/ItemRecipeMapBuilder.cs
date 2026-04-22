#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;
using Work.Code.Crafting;
using Work.LKW.Code.Items.ItemInfo;

public static class ItemRecipeMapBuilder
{
    [MenuItem("Tools/Crafting/Build ItemRecipeMap")]
    public static void Build()
    {
        var map = AssetDatabase.LoadAssetAtPath<CraftableItemListSO>(
            "Assets/Work/KIMMIN/06_SO/ItemRecipeMap.asset");

        var allItems = AssetDatabase.FindAssets("t:ItemDataSO")
            .Select(g => AssetDatabase.LoadAssetAtPath<ItemDataSO>(
                AssetDatabase.GUIDToAssetPath(g)))
            .ToList();

        var allRecipes = AssetDatabase.FindAssets("t:CraftTreeSO")
            .Select(g => AssetDatabase.LoadAssetAtPath<CraftTreeSO>(
                AssetDatabase.GUIDToAssetPath(g)))
            .ToList();

        map.recipes.Clear();

        foreach (var item in allItems)
        {
            var entry = new CraftableItemListSO.Recipe
            {
                item = item
            };

            foreach (var recipe in allRecipes)
            {
                int ingredientCount = recipe.isBinary ? 2 : 3;
                if (recipe.nodeList.Count <= 1) continue;

                var ingredients = recipe.nodeList.Skip(1)
                    .Take(Mathf.Min(ingredientCount, recipe.nodeList.Count - 1));

                if (ingredients.Any(n => n.Item == item))
                {
                    var resultItem = recipe.Item;

                    if (resultItem != null && !entry.results.Contains(resultItem))
                    {
                        entry.results.Add(resultItem);
                    }
                }
            }

            if (entry.results.Count > 0)
                map.recipes.Add(entry);
        }

        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssets();
        Debug.Log("========ItemRecipeMap Build Complete========");
    }
}
#endif