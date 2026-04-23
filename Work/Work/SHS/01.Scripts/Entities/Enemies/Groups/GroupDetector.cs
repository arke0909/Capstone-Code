using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using SHS.Scripts.Entities.Players;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Groups
{
    public class GroupDetector : MonoBehaviour, IContainerComponent
    {
        private EntitySensor _sensor;
        private GroupProvider _groupProvider;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _sensor = componentContainer.Get<EntitySensor>();
            _groupProvider = componentContainer.Get<GroupProvider>();
        }

        private void Update()
        {
            Group currentGroup = _groupProvider.CurrentGroup;
            if (currentGroup == null || currentGroup.Members.Count <= 1)
                JoinOrCreateGroup();
        }

        private void JoinOrCreateGroup()
        {
            foreach (Entity targetEntity in _sensor.DetectedEntities)
            {
                if (targetEntity is not Enemy targetEnemy)
                    continue;

                Group targetGroup = targetEnemy.GroupProvider.CurrentGroup;
                if (targetGroup != null)
                {
                    _groupProvider.SetGroup(targetGroup);
                    break;
                }
                else
                {
                    Group currentGroup = _groupProvider.CurrentGroup;
                    if (currentGroup == null)
                        currentGroup = CreateGroup();

                    targetEnemy.GroupProvider.SetGroup(currentGroup);
                }
            }
        }

        private Group CreateGroup()
        {
            Group currentGroup = new Group();
            _groupProvider.SetGroup(currentGroup);
            return currentGroup;
        }
    }
}