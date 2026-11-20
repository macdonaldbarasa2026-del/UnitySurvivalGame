using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private AudioClip attackSwingClip;
        [SerializeField] private AudioClip hitImpactClip;
        [SerializeField] private AudioClip harvestClip;
        [SerializeField] private AudioClip eatDrinkClip;
        [SerializeField] private AudioClip craftingSuccessClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
            }
        }

        public void PlaySfx(AudioClip clip, float volume = 1.0f, float pitchVariation = 0.1f)
        {
            if (clip == null || sfxSource == null) return;

            sfxSource.pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);
            sfxSource.PlayOneShot(clip, volume);
        }

        public void PlayFootstep()
        {
            if (footstepClips != null && footstepClips.Length > 0)
            {
                var clip = footstepClips[Random.Range(0, footstepClips.Length)];
                PlaySfx(clip, 0.4f, 0.15f);
            }
        }

        public void PlayAttackSwing() => PlaySfx(attackSwingClip, 0.7f);
        public void PlayHitImpact() => PlaySfx(hitImpactClip, 0.8f);
        public void PlayHarvest() => PlaySfx(harvestClip, 0.8f);
        public void PlayEatDrink() => PlaySfx(eatDrinkClip, 0.9f);
        public void PlayCraftingSuccess() => PlaySfx(craftingSuccessClip, 1.0f);
    }
}
