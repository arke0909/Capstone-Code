using System;
using EasyTransition;
using UnityEngine;
using Work.Code.Core;
using Work.Code.Core.Extension;

namespace Work.Code.Map
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private LayerMask whatIsPlayer;
        [SerializeField] private TransitionSettings transition;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.IsInLayer(whatIsPlayer))
            {
                TransitionManager.Instance().Transition(SceneDefine.HELICOPTER_SCENE, transition, 0f);
            }
        }
    }
}