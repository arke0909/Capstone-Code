using Code.Players;
using Code.UI.Core;
using Scripts.Combat.Datas;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Inventory
{
    public class InfoUI : MonoBehaviour, IUIElement<ReplaceBulletData>
    {
        [SerializeField] private TextMeshProUGUI ammoName;
        [SerializeField] private TextMeshProUGUI cntText;
        
        public void SetText(string text) => cntText.SetText(text);
        public void EnableFor(ReplaceBulletData bullet)
        {
            gameObject.SetActive(true);
            ammoName.text = bullet.bulletItem.bulletDataSO.itemName;
            cntText.text = bullet.bulletCnt.ToString();
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            ammoName.text = string.Empty;
            cntText.text = string.Empty;
        }
    }
}