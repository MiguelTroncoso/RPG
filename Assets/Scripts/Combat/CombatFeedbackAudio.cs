using System;
using UnityEngine;

namespace MmorpgPrototype
{
    // Lightweight procedural SFX keeps the prototype audible without adding
    // a third-party audio package or large binary files.
    public sealed class CombatFeedbackAudio : MonoBehaviour
    {
        public bool MusicEnabled = true;
        public float SfxVolume = 0.32f;
        public float MusicVolume = 0.06f;

        private AudioSource source;
        private AudioSource musicSource;
        private AudioClip attackClip;
        private AudioClip hitClip;
        private AudioClip criticalClip;
        private AudioClip missClip;
        private AudioClip damageClip;
        private AudioClip levelUpClip;
        private AudioClip skillClip;
        private AudioClip musicClip;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = SfxVolume;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = MusicVolume;

            attackClip = LoadOrCreate("Audio/Kenney/knifeSlice", "attack", 190f, 0.09f, 0.12f);
            hitClip = LoadOrCreate("Audio/Kenney/metalPot1", "hit", 340f, 0.11f, 0.16f);
            criticalClip = LoadOrCreate("Audio/Kenney/metalPot2", "critical", 680f, 0.18f, 0.2f);
            missClip = LoadOrCreate("Audio/Kenney/dropLeather", "miss", 130f, 0.08f, 0.1f);
            damageClip = LoadOrCreate("Audio/Kenney/dropLeather", "damage", 110f, 0.16f, 0.18f);
            levelUpClip = LoadOrCreate("Audio/Kenney/handleCoins", "level_up", 520f, 0.3f, 0.2f);
            skillClip = LoadOrCreate("Audio/Kenney/bookOpen", "skill", 430f, 0.16f, 0.16f);
            musicClip = Resources.Load<AudioClip>("Audio/Kenney/ambient_preview") ?? CreateMusicLoop();
            if (MusicEnabled)
            {
                musicSource.clip = musicClip;
                musicSource.Play();
            }
        }

        public void PlayAttack() => Play(attackClip, 0.95f);
        public void PlayHit(bool critical) => Play(critical ? criticalClip : hitClip, critical ? 1.05f : 1f);
        public void PlayMiss() => Play(missClip, 0.9f);
        public void PlayPlayerDamage() => Play(damageClip, 0.88f);
        public void PlayLevelUp() => Play(levelUpClip, 1f);
        public void PlaySkill() => Play(skillClip, 1.08f);

        public void ToggleMusic()
        {
            MusicEnabled = !MusicEnabled;
            if (musicSource == null)
            {
                return;
            }

            if (MusicEnabled)
            {
                musicSource.clip = musicClip;
                musicSource.Play();
            }
            else
            {
                musicSource.Stop();
            }
        }

        public void SetSfxVolume(float volume)
        {
            SfxVolume = Mathf.Clamp01(volume);
            if (source != null)
            {
                source.volume = SfxVolume;
            }
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = MusicVolume;
            }
        }

        private void Play(AudioClip clip, float pitch)
        {
            if (source == null || clip == null)
            {
                return;
            }

            source.pitch = pitch;
            source.PlayOneShot(clip);
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / sampleRate;
                var envelope = Mathf.Clamp01(1f - t / duration);
                var sweep = frequency * (1f + 0.12f * envelope);
                samples[i] = Mathf.Sin(2f * Mathf.PI * sweep * t) * envelope * volume;
            }

            var clip = AudioClip.Create($"Prototype SFX {name}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip LoadOrCreate(string resourcePath, string name, float frequency, float duration, float volume)
        {
            return Resources.Load<AudioClip>(resourcePath) ?? CreateTone(name, frequency, duration, volume);
        }

        private static AudioClip CreateMusicLoop()
        {
            const int sampleRate = 22050;
            const float duration = 8f;
            var sampleCount = Mathf.RoundToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = (float)i / sampleRate;
                var fade = Mathf.Min(Mathf.Clamp01(t / 0.35f), Mathf.Clamp01((duration - t) / 0.35f));
                var pad = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.42f;
                var fifth = Mathf.Sin(2f * Mathf.PI * 164.81f * t) * 0.2f;
                var octave = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.1f;
                samples[i] = (pad + fifth + octave) * fade * 0.16f;
            }

            var clip = AudioClip.Create("Prototype Ambient Loop", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
