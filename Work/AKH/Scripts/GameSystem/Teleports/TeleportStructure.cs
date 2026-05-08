using Code.UI.Minimap.Core;
using EPOOutline;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.ItemContainers;

namespace Scripts.GameSystem.Teleports
{
    public class TeleportStructure : InteractableStructure
    {
        [SerializeField] private Sprite teleportIcon;

        protected override void Start()
        {
            base.Start();

            MinimapUtil.AddToMinimap(this, ElementType.Marker, teleportIcon, true, transform.position);
            MinimapUtil.AddToMinimap(this, ElementType.Teleport, null, false, transform.position);
        }

        public override void Interact(Entity interactor)
        {
            //팝업 띄워주면 될듯
            Debug.Log("ASdasd");
        }
    }
}
