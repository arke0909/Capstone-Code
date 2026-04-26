using System.Collections.Generic;
using Ami.BroAudio;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Players;
using Code.UI.Core;
using Scripts.Combat.Datas;
using Scripts.Players.States;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Misc;

namespace Code.UI.Inventory
{
    public class BulletAmmoUI : UIBase, IUIElement<GunItem>
    {
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private Image selectedImage;
        [SerializeField] private GameObject replaceBulletParentGO;
        [SerializeField] private PlayerInputSO playerInput;
        [SerializeField] private DynamicText ammoText;
        [SerializeField] private SoundID emptySoundID;
        
        private RectTransform BulletImageRect => selectedImage.transform as RectTransform;
        private InfoUI[] _infoUIs;
        private List<ReplaceBulletData> _replaceBulletDatas;
        private int _currentReplaceDataIndex = 0;
        private int _totalCnt;
        private bool _isActive;
        
        protected override void Awake()
        {
            _infoUIs = GetComponentsInChildren<InfoUI>();
            EventBus.Subscribe<ChangeHandlingEvent>(HandleChangeWeapon);
            EventBus.Subscribe<NoAmmoSoundEvent>(HandleNoAmmo);
            
            UIUtility.FadeUI(gameObject, 0, true);
            UIUtility.FadeUI(replaceBulletParentGO, 0, true);
        }

        private void HandleNoAmmo(NoAmmoSoundEvent evt)
        {
            BroAudio.Play(emptySoundID);
            ammoText.PlayEffect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus.Unsubscribe<ChangeHandlingEvent>(HandleChangeWeapon);
            EventBus.Unsubscribe<NoAmmoSoundEvent>(HandleNoAmmo);
        }

        #region Replace Ammo
        private void HandleIndexUp()
        {
            _currentReplaceDataIndex = Mathf.Min(_totalCnt - 1, _currentReplaceDataIndex + 1);
            SetSelectedImagePos(_currentReplaceDataIndex);
        }
        
        private void HandleIndexDown()
        {
            _currentReplaceDataIndex = Mathf.Max(0, _currentReplaceDataIndex - 1);
            SetSelectedImagePos(_currentReplaceDataIndex);
        }
        
        private void HandleSelectReplaceBullet()
        {
            //이벤트 발행?
            EventBus<ReplaceBulletEvent>.Raise(new ReplaceBulletEvent(_replaceBulletDatas[_currentReplaceDataIndex].bulletItem));
            OffReplaceBulletUI();
        }

        private void InputClear()
        {
            playerInput.OnUpBulletListPressed -= HandleIndexUp;
            playerInput.OnDownBulletListPressed -= HandleIndexDown;
            playerInput.OnBulletShowPressed -= HandleSelectReplaceBullet;
        }

        private void OffReplaceBulletUI()
        {
            InputClear();
            EventBus.Unsubscribe<OffReplaceBulletUI>(HandleOffReplaceBulletUI);
            UIUtility.FadeUI(replaceBulletParentGO, fadeDuration, true);
        }

        private void SetSelectedImagePos(int idx)
        {
            if(idx < 0 || idx >= _infoUIs.Length) return;
            
            RectTransform rect = _infoUIs[idx].transform as RectTransform;
            BulletImageRect.position = rect.position;
        }

        private void HandleReplaceBulletList(ReplaceBulletListEvent evt)
        {
            _replaceBulletDatas = evt.Data;
            
            foreach (var infoUI in _infoUIs)
            {
                infoUI.ClearUI();
            }

            _totalCnt = evt.Data.Count;

            if(_totalCnt == 0) return;

            playerInput.OnUpBulletListPressed += HandleIndexUp;
            playerInput.OnDownBulletListPressed += HandleIndexDown;
            playerInput.OnBulletShowPressed += HandleSelectReplaceBullet;
            
            
            for (int i = 0; i < _totalCnt; ++i)
            {
                _infoUIs[i].EnableFor(evt.Data[i]);
            }
            
            SetSelectedImagePos(evt.Idx);
            
            EventBus.Subscribe<OffReplaceBulletUI>(HandleOffReplaceBulletUI);

            UIUtility.FadeUI(replaceBulletParentGO, fadeDuration, false);
        }
        

        #endregion
        
        public void EnableFor(GunItem element)
        {
            //int totalAmmo = Mathf.Max(element.GunItemData.maxBullet, element.currentBulletItem.Stack);
            SetAmmoText(element.CurrentBulletCnt, element.GunItemData.maxAmmoCapacity);
        }

        public void ClearUI() { }
        
        private void HandleChangeWeapon(ChangeHandlingEvent evt)
        {
            if (evt.EquipableItem is GunItem gun)
            {
                if (!_isActive)
                {
                    _isActive = true;
                    
                    EventBus.Subscribe<OffReplaceBulletUI>(HandleOffReplaceBulletUI);
                    EventBus.Subscribe<AmmoUpdateEvent>(HandleGunFire);
                    EventBus.Subscribe<ReplaceBulletListEvent>(HandleReplaceBulletList);

                    UIUtility.FadeUI(gameObject, fadeDuration, false);
                }
        
                EnableFor(gun);
            }
            else
            {
                if (_isActive)
                {
                    _isActive = false;
                    
                    EventBus.Unsubscribe<OffReplaceBulletUI>(HandleOffReplaceBulletUI);
                    EventBus.Unsubscribe<AmmoUpdateEvent>(HandleGunFire);
                    EventBus.Unsubscribe<ReplaceBulletListEvent>(HandleReplaceBulletList);
                    
                    UIUtility.FadeUI(gameObject, fadeDuration, true);
                }
            }
        }
        
        private void HandleGunFire(AmmoUpdateEvent evt)
        {
            SetAmmoText(evt.CurrentAmmo, evt.TotalAmmo);
        }
        
        private void HandleOffReplaceBulletUI(OffReplaceBulletUI evt)
        {
            OffReplaceBulletUI();
        }

        private void SetAmmoText(int currentAmmo, int totalAmmo)
        {
            ammoText.SetText($"{currentAmmo}/{totalAmmo}");
            ammoText.Text.color = currentAmmo == 0 ? UIDefine.RedColor : Color.white;
        }
    }
}