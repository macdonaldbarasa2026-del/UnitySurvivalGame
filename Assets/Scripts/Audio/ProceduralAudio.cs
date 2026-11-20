using System;
using System.Reflection;
using UnityEngine;

namespace SurvivalGame.Audio
{
    /// <summary>
    /// Generates every sound at runtime from raw samples. No downloaded audio files,
    /// so the game is completely offline and has no external asset dependencies.
    /// </summary>
    public static class ProceduralAudio
    {
        private const int SampleRate = 22050;

        public static void Install(SoundManager manager)
        {
            if (manager == null) return;

            SetField(manager, "footstepClips", new[]
            {
                MakeFootstep(0.11f, 74f, 1),
                MakeFootstep(0.13f, 68f, 2),
                MakeFootstep(0.10f, 82f, 3)
            });
            SetField(manager, "attackSwingClip", MakeSwing());
            SetField(manager, "hitImpactClip", MakeHit());
            SetField(manager, "harvestClip", MakeHarvest());
            SetField(manager, "eatDrinkClip", MakeEat());
            SetField(manager, "craftingSuccessClip", MakeCraft());

            AudioSource music = GetField<AudioSource>(manager, "musicSource");
            if (music != null)
            {
                music.clip = MakeMusic();
                music.loop = true;
                music.volume = 0.22f;
                music.Play();
            }

            AudioSource ambient = GetField<AudioSource>(manager, "ambientSource");
            if (ambient != null)
            {
                ambient.clip = MakeWind();
                ambient.loop = true;
                ambient.volume = 0.12f;
                ambient.Play();
            }
        }

        private static AudioClip MakeFootstep(float duration, float thumpHz, int seed)
        {
            float[] data = new float[(int)(duration * SampleRate)];
            var random = new System.Random(seed * 7919);
            float previous = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Exp(-t * 34f);
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                noise = previous * 0.6f + noise * 0.4f;
                previous = noise;
                float thump = Mathf.Sin(2f * Mathf.PI * thumpHz * t) * Mathf.Exp(-t * 26f);
                data[i] = Mathf.Clamp((noise * 0.5f + thump * 0.5f) * envelope, -1f, 1f);
            }
            return Clip("Footstep" + seed, data);
        }

        private static AudioClip MakeSwing()
        {
            float duration = 0.26f;
            float[] data = new float[(int)(duration * SampleRate)];
            var random = new System.Random(4242);
            float previous = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)SampleRate;
                float p = t / duration;
                float envelope = Mathf.Sin(p * Mathf.PI) * (1f - p * 0.4f);
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                noise = previous * 0.55f + noise * 0.45f;
                previous = noise;
                float sweep = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(720f, 180f, p) * t);
                data[i] = Mathf.Clamp((noise * 0.65f + sweep * 0.35f) * envelope * 0.8f, -1f, 1f);
            }
            return Clip("Swing", data);
        }

        private static AudioClip MakeHit()
        {
            float duration = 0.2f;
            float[] data = new float[(int)(duration * SampleRate)];
            var random = new System.Random(99);
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Exp(-t * 22f);
                float low = Mathf.Sin(2f * Mathf.PI * 118f * t);
                float crack = (float)(random.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 60f);
                data[i] = Mathf.Clamp((low * 0.6f + crack * 0.4f) * envelope, -1f, 1f);
            }
            return Clip("Hit", data);
        }

        private static AudioClip MakeHarvest()
        {
            float duration = 0.32f;
            float[] data = new float[(int)(duration * SampleRate)];
            var random = new System.Random(777);
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Exp(-t * 12f);
                float wood = Mathf.Sin(2f * Mathf.PI * 210f * t) * 0.55f
                           + Mathf.Sin(2f * Mathf.PI * 317f * t) * 0.25f;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0) * 0.2f * Mathf.Exp(-t * 30f);
                data[i] = Mathf.Clamp((wood + noise) * envelope, -1f, 1f);
            }
            return Clip("Harvest", data);
        }

        private static AudioClip MakeEat()
        {
            float duration = 0.22f;
            float[] data = new float[(int)(duration * SampleRate)];
            var random = new System.Random(31337);
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)SampleRate;
                float chew = (Mathf.Sin(t * 92f) > 0f) ? 1f : 0.25f;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                data[i] = Mathf.Clamp(noise * chew * Mathf.Exp(-t * 9f) * 0.5f, -1f, 1f);
            }
            return Clip("Eat", data);
        }

        private static AudioClip MakeCraft()
        {
            float duration = 0.55f;
            float[] data = new float[(int)(duration * SampleRate)];
            for (int i = 0; i < data.Length; i++)
            {
                float t = i / (float)SampleRate;
                float tone = t < duration * 0.5f
                    ? Mathf.Sin(2f * Mathf.PI * 440f * t)
                    : Mathf.Sin(2f * Mathf.PI * 659.25f * t);
                float envelope = Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                data[i] = tone * envelope * 0.4f;
            }
            return Clip("Craft", data);
        }

        private static AudioClip MakeMusic()
        {
            float duration = 12f;
            int length = (int)(duration * SampleRate);
            float[] data = new float[length];
            float[] chord = { 110f, 130.81f, 164.81f, 220f, 261.63f };
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float slow = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * (t / 12f));
                float sample = 0f;
                for (int n = 0; n < chord.Length; n++)
                {
                    float phase = 2f * Mathf.PI * chord[n] * t;
                    sample += Mathf.Sin(phase) * (0.16f / chord.Length)
                            + Mathf.Sin(phase * 2f) * (0.04f / chord.Length);
                }
                sample *= 0.55f + 0.45f * slow;
                // Smooth the loop seam so there is no click.
                float fade = Mathf.Clamp01(Mathf.Min(t, duration - t) * 2f);
                data[i] = sample * fade;
            }
            return Clip("AmbientMusic", data);
        }

        private static AudioClip MakeWind()
        {
            float duration = 6f;
            int length = (int)(duration * SampleRate);
            float[] data = new float[length];
            var random = new System.Random(2024);
            float filtered = 0f;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered = filtered * 0.97f + noise * 0.03f;
                float gust = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * (t / 6f));
                float fade = Mathf.Clamp01(Mathf.Min(t, duration - t) * 2f);
                data[i] = filtered * gust * fade * 1.8f;
            }
            return Clip("Wind", data);
        }

        private static AudioClip Clip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) field.SetValue(target, value);
        }

        private static T GetField<T>(object target, string name) where T : class
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            return field == null ? null : field.GetValue(target) as T;
        }
    }
}
