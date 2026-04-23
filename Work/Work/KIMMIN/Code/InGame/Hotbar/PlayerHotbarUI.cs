using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InGame.Hotbar;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Code.Hotbar
{
    public class PlayerHotbarUI : MonoBehaviour
    { 
        private HotbarUI[] _hotbars;

        private void Awake()
        {
            _hotbars = GetComponentsInChildren<HotbarUI>();

            for (int i = 0; i < _hotbars.Length; i++)
            {
                _hotbars[i].SetIndex(i);
            }
            
            EventBus.Subscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
        }

        private void HandleUpdateHotbar(UpdateHotbarUIEvent evt)
        {
            for(int i = 0; i < _hotbars.Length; i++)
            {
                if (evt.EquipSlots[i].Item != null)
                {
                    _hotbars[i].EnableFor(evt.EquipSlots[i]);
                }
                else
                    _hotbars[i].Clear();
            }
        }
    }
}