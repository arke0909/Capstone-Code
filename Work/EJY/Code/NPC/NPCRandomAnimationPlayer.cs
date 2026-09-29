using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.NPC
{
    [RequireComponent(typeof(Animator))]
    public class NPCRandomAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip[] randomClips;

        private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _clipOverrides = new();
        private AnimatorOverrideController _overrideController;
        private Coroutine _playRoutine;
        private int _defaultStateHash;
        private int _previousClipIndex = -1;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                Debug.LogError("Animator is required for random NPC animation playback.", this);
                enabled = false;
                return;
            }

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                Debug.LogError("RuntimeAnimatorController is required for random NPC animation playback.", this);
                enabled = false;
                return;
            }

            _overrideController = new AnimatorOverrideController(controller);
            CacheSourceClips(controller);

            if (_clipOverrides.Count == 0)
            {
                Debug.LogError("NPC random animation player needs at least one source clip in the controller.", this);
                enabled = false;
                return;
            }

            animator.runtimeAnimatorController = _overrideController;
            animator.Rebind();
            animator.Update(0f);
            _defaultStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }

        private void OnEnable()
        {
            if (_overrideController == null)
            {
                return;
            }

            _playRoutine = StartCoroutine(PlayRandomLoop());
        }

        private void OnDisable()
        {
            if (_playRoutine == null)
            {
                return;
            }

            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        private IEnumerator PlayRandomLoop()
        {
            while (true)
            {
                if (!TryGetRandomClip(out AnimationClip clip))
                {
                    _playRoutine = null;
                    yield break;
                }

                ApplyClip(clip);
                animator.Play(_defaultStateHash, 0, 0f);
                animator.Update(0f);

                if (clip.length <= 0f)
                {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(clip.length);
            }
        }

        private void ApplyClip(AnimationClip clip)
        {
            for (int i = 0; i < _clipOverrides.Count; i++)
            {
                AnimationClip sourceClip = _clipOverrides[i].Key;
                _clipOverrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(sourceClip, clip);
            }

            _overrideController.ApplyOverrides(_clipOverrides);
        }

        private bool TryGetRandomClip(out AnimationClip clip)
        {
            if (randomClips == null || randomClips.Length == 0)
            {
                clip = null;
                return false;
            }

            int validClipCount = 0;
            for (int i = 0; i < randomClips.Length; i++)
            {
                if (randomClips[i] != null)
                {
                    validClipCount++;
                }
            }

            if (validClipCount == 0)
            {
                clip = null;
                return false;
            }

            int selectedOrder = Random.Range(0, validClipCount);
            int clipIndex = -1;
            int currentOrder = 0;
            for (int i = 0; i < randomClips.Length; i++)
            {
                if (randomClips[i] == null)
                {
                    continue;
                }

                if (currentOrder == selectedOrder)
                {
                    clipIndex = i;
                    break;
                }

                currentOrder++;
            }

            if (validClipCount > 1 && clipIndex == _previousClipIndex)
            {
                clipIndex = GetNextValidClipIndex(clipIndex);
            }

            _previousClipIndex = clipIndex;
            clip = randomClips[clipIndex];
            return true;
        }

        private int GetNextValidClipIndex(int startIndex)
        {
            for (int offset = 1; offset < randomClips.Length; offset++)
            {
                int clipIndex = (startIndex + offset) % randomClips.Length;
                if (randomClips[clipIndex] != null)
                {
                    return clipIndex;
                }
            }

            return startIndex;
        }

        private void CacheSourceClips(RuntimeAnimatorController controller)
        {
            AnimationClip[] controllerClips = controller.animationClips;
            for (int i = 0; i < controllerClips.Length; i++)
            {
                AnimationClip clip = controllerClips[i];
                if (clip == null || HasSourceClip(clip))
                {
                    continue;
                }

                _clipOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip, clip));
            }
        }

        private bool HasSourceClip(AnimationClip sourceClip)
        {
            for (int i = 0; i < _clipOverrides.Count; i++)
            {
                if (_clipOverrides[i].Key == sourceClip)
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }
#endif
    }
}
