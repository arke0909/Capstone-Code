using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using Scripts.Effects;
using Scripts.GameSystem;
using UnityEngine;
using Code.ETC.MapObjects;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Code.ETC.MapObjects
{
public class BreakableObject : HittableObject
{
    [Inject]
    private PoolManagerMono _poolManagerMono;

    [Header("Fragments")]
    [SerializeField] private List<GameObject> fragmentPrefabs;
    [SerializeField] private int minFragments = 3;
    [SerializeField] private int maxFragments = 6;
    [SerializeField] private float minFragmentForce = 0.1f;
    [SerializeField] private float maxFragmentForce = 0.5f;
    [SerializeField] private float minFragmentTorque = 0.1f;
    [SerializeField] private float maxFragmentTorque = 0.5f;

    [Header("Effect")]
    [SerializeField] private PoolItemSO breakEffect;
    [SerializeField] private SoundID breakSoundID;

    [Header("Fade")]
    [SerializeField] private float minFadeDelay = 0.5f;
    [SerializeField] private float maxFadeDelay = 1f;
    [SerializeField] private float fadeDuration = 0.5f;
    private WaitForSeconds _tweenKillWaitForSecond;

    [Header("Item Drop")]
    [SerializeField] private ItemDropper itemDropper;
    [SerializeField] private float dropRadius = 1f;

    protected override void Awake() 
        => _tweenKillWaitForSecond = new WaitForSeconds(fadeDuration);

    protected override void OnDeath()
    {
        StartCoroutine(BreakCoroutine());
    }

    private IEnumerator BreakCoroutine()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        PlaySoundAndEffect();

        var fragments = SpawnFragment();

        DropItem();

        yield return new WaitForSeconds(Random.Range(minFadeDelay, maxFadeDelay));
        yield return FadeOutFragments(fragments, fadeDuration);

        foreach (var f in fragments)
            if (f != null) Destroy(f);

        Destroy(gameObject);
    }

    private void PlaySoundAndEffect()
    {
        if (breakSoundID.IsValid())
            BroAudio.Play(breakSoundID, transform.position);

        if (breakEffect != null)
        {
            var effect = _poolManagerMono.Pop<PoolingEffect>(breakEffect);
            effect?.PlayVFX(transform.position, Quaternion.identity);
        }
    }

    private void DropItem()
    {
        if (itemDropper != null)
        {
            Vector2 offset = Random.insideUnitCircle * dropRadius;
            Vector3 to = transform.position + new Vector3(offset.x, 0f, offset.y);
            itemDropper.Drop(transform.position, to);
        }
    }

    private List<GameObject> SpawnFragment()
    {
        int count = Random.Range(minFragments, maxFragments + 1);
        var fragments = new List<GameObject>(count);
        
        if (fragmentPrefabs == null || fragmentPrefabs.Count == 0) return fragments;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * 0.1f;
            var prefab = fragmentPrefabs[Random.Range(0, fragmentPrefabs.Count)];
            var fragment = Instantiate(prefab, transform.position + spawnOffset, Random.rotation);
            fragments.Add(fragment);

            if (fragment.TryGetComponent(out Rigidbody rb))
            {
                Vector3 dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y);

                float fragmentForce = Random.Range(minFragmentForce, maxFragmentForce);
                float fragmentTorque = Random.Range(minFragmentTorque, maxFragmentTorque);
                rb.AddForce(dir * fragmentForce, ForceMode.Impulse);
                rb.AddTorque(Random.onUnitSphere * fragmentTorque, ForceMode.Force);
            }
        }

        return fragments;
    }

    private IEnumerator FadeOutFragments(List<GameObject> fragments, float duration)
    {
        var materials = new List<Material>();
        foreach (var f in fragments)
        {
            if (f == null) continue;
            foreach (var r in f.GetComponentsInChildren<Renderer>())
                materials.AddRange(r.materials);
        }

        var tweens = new List<Tweener>(materials.Count);
        foreach (var mat in materials)
            tweens.Add(mat.DOFade(0f, duration));

        yield return _tweenKillWaitForSecond;

        foreach (var t in tweens)
            t?.Kill();
    }
}
}