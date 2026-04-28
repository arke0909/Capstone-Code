using Scripts.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Groups
{

    public class Group
    {
        public HashSet<Enemy> Members { get; private set; } = new HashSet<Enemy>();
        public event Action<Entity> OnTargetDetected;
        public bool IsCombat = false;
        public void Join(Enemy enemy)
        {
            Members.Add(enemy);
        }
        public void Leave(Enemy enemy)
        {
            Debug.Assert(Members.Contains(enemy), "존재하지 않는애를 지우려하다니");
            Members.Remove(enemy);
        }
        public void TargetDetect(Entity entity)
        {
            if (IsCombat)
                return;
            OnTargetDetected?.Invoke(entity);
            IsCombat = true;
        }
    }
}