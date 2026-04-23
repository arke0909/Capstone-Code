using Chipmunk.GameEvents;
using Code.Events;
using UnityEngine;

namespace Code.UI.Minimap.Core
{
    public static class MinimapUtil
    {
        public static string AddToMinimap(object owner, ElementType type,Sprite icon, bool syncChildScale = true, Vector3 worldInitPos = default )
        {
            var data = new MinimapElementData(
                owner,
                type,
                icon, 
                syncChildScale, 
                worldInitPos != default
            );
            
            var evt = new AddMinimapElementEvent(
               data,
               worldInitPos
            );
            
            Bus.Raise(evt);

            return data.Id;
        }

        
        public static void RemoveFromMinimap(this string id)
        {
            if(string.IsNullOrEmpty(id)) return;
            Bus.Raise(new RemoveMinimapElementEvent(id));
        }
        
    }
}