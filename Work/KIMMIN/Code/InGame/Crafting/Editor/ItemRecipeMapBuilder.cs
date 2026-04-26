#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;
using Work.Code.Craft;
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
            Recipe recipe = new Recipe { item = item };

            foreach (CraftTreeSO tree in allRecipes)
            {
                int ingredientCount = tree.isBinary ? 2 : 3;
                
                if (tree.nodeList.Count <= 1)
                    continue;

                var craftItems = tree.nodeList.Skip(1)
                    .Take(Mathf.Min(ingredientCount, tree.nodeList.Count - 1));

                if (craftItems.Any(n => n.Item == item))
                {
                    ItemDataSO resultItem = tree.Item;

                    if (resultItem != null && !recipe.results.Contains(resultItem))
                    {
                        recipe.results.Add(resultItem);
                    }
                }
            }

            if (recipe.results.Count > 0)
                map.recipes.Add(recipe);
        }

        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssets();
        Debug.Log("========ItemRecipeMap Build Complete========");
    }
}
#endif