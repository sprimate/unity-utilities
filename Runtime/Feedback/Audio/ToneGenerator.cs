/* Copyright

    VR Rehab, Inc Confidential
    __________________
    
     Copyright 2019 - VR Rehab, Inc
     All Rights Reserved.
    
    NOTICE:  All information contained herein is, and remains
    the property of VR Rehab, Inc and its subsidiaries,
    if any.  The intellectual and technical concepts contained
    herein are proprietary to VR Rehab, Inc
    and its subsidiaries and may be covered by U.S. and Foreign Patents,
    patents in process, and are protected by trade secret or copyright law.
    Dissemination of this information or reproduction of this material
    is strictly forbidden unless prior written permission is obtained
    from VR Rehab, Inc.
    contact: smann@virtualrealityrehab.com

*/

using System;
using System.Collections.Generic;
using UnityEngine;
using Sprimate.Feedback.Services;

namespace Sprimate.Feedback.Audio
{
    public class ToneGenerator : AWaveformService
    {        
        public WaveForm waveForm;        
        public int sampleRate = 44100;
        
        double freq;
        double duration;
        float amplitude;

        public uint maxCacheBytes = 1000000; // 1 MB

        int removedCount;
        const int MAX_REMOVED_BEFORE_WARNING = 10;

        Dictionary<string, AudioClipCacheable> tonesCache = 
            new Dictionary<string, AudioClipCacheable>();
 
        class AudioClipCacheable
        {
            public AudioClip audioClip;
            public uint sizeBytes;
            public float lastUsed;
        }

        List<AudioSource> audioSources = new List<AudioSource>();

        public enum WaveForm
        {
            Sin,
            Square,
            Triangle,
            SawTooth
        }

        public override void ExecuteWave(
            double durationSeconds, double frequency, uint amplitudePercent)
        {
            freq = frequency;
            duration = durationSeconds;
            amplitude = amplitudePercent / 100f;
            CreateTone();
        }

        public uint GetTonesCacheSizeBytes()
        {
            uint size = 0;
            foreach (var item in tonesCache)
            {
                size += item.Value.sizeBytes;
            }

            return size;
        }

        string GetOldestClipKey()
        {
            float time = float.PositiveInfinity;
            string key = "";

            foreach (var item in tonesCache)
            {
                if (item.Value.lastUsed < time)
                {
                    time = item.Value.lastUsed;
                    key = item.Key;
                }
            }

            return key;
        }

        private void CreateTone()
        {
            // generate name based on settings.

            string clipName = 
                $"{waveForm}_{freq.ToString("N2")}_{sampleRate}_{duration.ToString("N2")}";

            AudioClip clip = null;
            if (tonesCache.ContainsKey(clipName))
                clip = tonesCache[clipName].audioClip;

            if (clip == null)
            {
                //we have to round this number so we can set our samples in seconds
                //44000 samples is ruffly 1 second
                var sampleLength = (int)Math.Round(sampleRate * duration);
                float[] samples = new float[sampleLength];

                for (int i = 0; i < samples.Length; i++)
                {
                    switch (waveForm)
                    {
                        case WaveForm.SawTooth:
                            samples[i] = CreateSawtooth(i);
                            break;

                        case WaveForm.Sin:
                            samples[i] = CreateSine(i);
                            break;

                        case WaveForm.Square:
                            samples[i] = CreateSquare(i);
                            break;

                        case WaveForm.Triangle:
                            samples[i] = CreateTriangle(i);
                            break;

                        default:
                            throw new NotImplementedException(
                                "WaveForm type is missing this shouldn't happen. WaveForm type: "
                                + waveForm.ToString());
                    }
                }

                clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
                clip.SetData(samples, 0);

                HandleTonesCache(clipName, clip, (uint)sampleLength * 4); // 4 bytes per float
            }

            HandleClipPlayer(clip);
        }

        private float CreateSine(int index)
        {
            return amplitude * Mathf.Sin(Mathf.PI * 2 * index * (float)freq / sampleRate);
        }

        private float CreateSquare(int index)
        {

            return (Mathf.Repeat(index * (float)freq / sampleRate, 1) > 0.5f) ? 
                1f * amplitude : -1f * amplitude;
        }

        private float CreateTriangle(int index)
        {
            return amplitude * Mathf.PingPong(index * 2f * (float)freq / sampleRate, 1) * 2f - 1f;
        }

        private float CreateSawtooth(int index)
        {
            return amplitude * Mathf.Repeat(index * (float)freq / sampleRate, 1) * 2f - 1f;
        }

        void HandleTonesCache(string name, AudioClip clip, uint sampleSizeBytes)
        {
            if (!tonesCache.ContainsKey(name))
                tonesCache.Add(name, new AudioClipCacheable());

            if (tonesCache[name] == null)
                tonesCache[name] = new AudioClipCacheable();

            tonesCache[name].audioClip = clip;
            tonesCache[name].lastUsed = Time.realtimeSinceStartup;
            tonesCache[name].sizeBytes = sampleSizeBytes;

            int maxIters = 100;
            int iter = 0;
            while (GetTonesCacheSizeBytes() > maxCacheBytes)
            {
                if (iter > maxIters)
                    break;

                string keyToKill = GetOldestClipKey();
                if (!string.IsNullOrEmpty(keyToKill))
                {
                    removedCount++;

                    if (removedCount >= MAX_REMOVED_BEFORE_WARNING)
                    {
                        Debug.LogWarning($"{GetType()} on {name} has removed {removedCount} " +
                            $"clips from cache. Consider increasing the cache size or reducing " +
                            $"the sample rate.");
                        removedCount = int.MinValue;
                    }

                    tonesCache.Remove(keyToKill);
                }

                iter++;
            }
        }

        void HandleClipPlayer(AudioClip clip)
        {
            AudioSource freeSource = null;
            foreach (var item in audioSources)
            {
                if (!item.isPlaying)
                {
                    freeSource = item;
                }
            }

            if (freeSource == null)
            {
                freeSource = gameObject.AddComponent<AudioSource>();
                audioSources.Add(freeSource);
            }

            freeSource.PlayOneShot(clip);
        }
    }
}
