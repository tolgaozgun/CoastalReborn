using System;
using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Stub implementation of the audio service.
    /// </summary>
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Audio Configuration")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        private float masterVolume = 1f;
        private float musicVolume = 0.8f;
        private float sfxVolume = 1f;
        private bool isMuted = false;

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                UpdateAudioLevels();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                UpdateAudioLevels();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                UpdateAudioLevels();
            }
        }

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                UpdateAudioLevels();
            }
        }

        public void Initialize()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            LoadSettings();
        }

        public void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.pitch = pitch;
                sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
            }
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f, float fadeDuration = 1f)
        {
            if (clip != null && musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.volume = volume * musicVolume * masterVolume;
                musicSource.Play();
            }
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            PlaySFX(clip, Vector3.zero, volume * 0.7f);
        }

        private void UpdateAudioLevels()
        {
            if (isMuted)
            {
                if (musicSource != null) musicSource.volume = 0f;
                if (sfxSource != null) sfxSource.volume = 0f;
            }
            else
            {
                if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
                if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
            }
        }

        private void LoadSettings()
        {
            // TODO: Load audio settings from PlayerPrefs
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            IsMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}