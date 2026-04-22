using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.ETC
{
    public class InteractableCircle : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;

        public void EnableCircle(bool isEnable)
        {
            gameObject.SetActive(isEnable);
        }
    }
}