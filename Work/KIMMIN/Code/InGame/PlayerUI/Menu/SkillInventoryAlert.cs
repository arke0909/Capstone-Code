using System;
using System.Linq;
using Code.UI.Core;
using Scripts.SkillSystem;
using UnityEngine;
using Work.Code.SkillInventory;

namespace Work.Code.PlayerUI.Menu
{
    public class SkillInventoryAlert : AlertController
    {
        [SerializeField] private SkillInventoryUI skillInventoryUI;

        protected override void Start()
        {
            base.Start();
            skillInventoryUI.OnChangeInventory += HandleSkillInventoryChanged;
        }

        private void HandleSkillInventoryChanged(Skill[] skills)
        {
            bool canShowAlert = false;

            foreach (Skill skill in skills)
            {
                if(skill.SkillData != null)
                    canShowAlert = true;
            }

            if (canShowAlert)
                SetAlert(true);
            else
                SetAlert(false);
        }

        private void OnDestroy()
        {
            skillInventoryUI.OnChangeInventory -= HandleSkillInventoryChanged;
        }
    }
}