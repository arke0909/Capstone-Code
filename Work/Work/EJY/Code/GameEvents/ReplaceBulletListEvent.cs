using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.Players;
using Scripts.Combat.Datas;

namespace Code.GameEvents
{
    public class ReplaceBulletListEvent : IEvent
    {
        public List<ReplaceBulletData> Data { get; }
        public int Idx { get; }
        
        public ReplaceBulletListEvent(List<ReplaceBulletData> data, int idx)
        {
            Data = data;
            Idx = idx;
        }
    }
}