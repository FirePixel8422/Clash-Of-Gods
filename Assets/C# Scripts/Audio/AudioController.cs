using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    public List<AudioSource> audioSources;

    public Audio_Type audioType;
    public enum Audio_Type
    {
        Master,
        SoundEffects,
        Music
    };


    public AudioClip[] clips;

    private int clipIndex;

    public OrderMode clipOrder;
    public enum OrderMode
    {
        InOrder,
        FullyRandom
    };

    public bool autoRefillSources;

    private float defVolume;
    private float defPitch;



    public void Init()
    {
        audioSources = GetComponents<AudioSource>().ToList();
        defVolume = audioSources[0].volume;
        defPitch = audioSources[0].pitch;
    }


    public void UpdateVolume(float main, float sfx, float music)
    {
        if(audioSources.Count == 0)
        {
            return;
        }

        foreach (AudioSource source in audioSources)
        {
            source.volume = main;
            if (audioType == Audio_Type.SoundEffects)
            {
                source.volume *= sfx;
            }
            if (audioType == Audio_Type.Music)
            {
                source.volume *= music;
            }
        }
        defVolume = audioSources[0].volume; 
        defPitch = audioSources[0].pitch;
    }


    public void Play(float volumeMultiplier = -1, float pitchMultiplier = -1)
    {
        if (clips.Length == 0)
        {
            return;
        }

        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
            {
                continue;
            }

            if (clipOrder == OrderMode.FullyRandom)
            {
                int r = Random.Range(0, clips.Length);
                source.clip = clips[r];
            }
            else
            {
                source.clip = clips[clipIndex];
                clipIndex += 1;
                if (clipIndex >= clips.Length)
                {
                    clipIndex = 0;
                }
            }

            if (volumeMultiplier > 0)
            {
                source.volume = defVolume * volumeMultiplier;
            }
            if (pitchMultiplier > 0)
            {
                source.pitch = defPitch * pitchMultiplier;
            }

            source.Play();
            return;
        }

        if (autoRefillSources)
        {
            AudioSource addedSource = transform.AddComponent<AudioSource>();
            audioSources.Add(addedSource);

            addedSource.Play();
        }
        else
        {
            Debug.LogWarning("too little audiosources on: " + gameObject.name);
        }
    }
}
