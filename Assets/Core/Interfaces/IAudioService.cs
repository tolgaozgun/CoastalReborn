using System;
using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Service for managing game audio.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Master volume level (0-1).
        /// </summary>
        float MasterVolume { get; set; }

        /// <summary>
        /// Music volume level (0-1).
        /// </summary>
        float MusicVolume { get; set; }

        /// <summary>
        /// SFX volume level (0-1).
        /// </summary>
        float SFXVolume { get; set; }

        /// <summary>
        /// Check if audio is muted.
        /// </summary>
        bool IsMuted { get; set; }

        /// <summary>
        /// Play a sound effect.
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="position">World position (for 3D sounds)</param>
        /// <param name="volume">Volume override (0-1)</param>
        /// <param name="pitch">Pitch modifier</param>
        void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f);

        /// <summary>
        /// Play a music track.
        /// </summary>
        /// <param name="clip">Music clip to play</param>
        /// <param name="loop">Whether to loop</param>
        /// <param name="volume">Volume override (0-1)</param>
        /// <param name="fadeDuration">Fade in duration</param>
        void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f, float fadeDuration = 1f);

        /// <summary>
        /// Stop the current music track.
        /// </summary>
        /// <param name="fadeDuration">Fade out duration</param>
        void StopMusic(float fadeDuration = 1f);

        /// <summary>
        /// Play a UI sound.
        /// </summary>
        /// <param name="clip">UI sound clip</param>
        /// <param name="volume">Volume override</param>
        void PlayUISound(AudioClip clip, float volume = 1f);

        /// <summary>
        /// Set audio mixers and load settings.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Save current audio settings.
        /// </summary>
        void SaveSettings();
    }
}