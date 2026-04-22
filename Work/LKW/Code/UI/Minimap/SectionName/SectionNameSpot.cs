using System;
using Code.UI.Minimap.Core;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

public enum SectionNameSize
{
    SMALL = 8,
    MEDIUM = 18,
    BIG = 28
}

namespace Code.UI.Minimap.SectionName
{
    public class SectionNameSpot : MonoBehaviour
    {
        [field:SerializeField] public string Name { get; set; }
        [field:SerializeField] public SectionNameSize Size { get; set; }
        [field:SerializeField] public SpawnArea Area { get; set; }


        private void OnValidate()
        {
            gameObject.name = $"Section_{Name}";
        }

        private void Start()
        {
            MinimapUtil.AddToMinimap(this, ElementType.SectionName, null, true, transform.position);
        }
    }
}