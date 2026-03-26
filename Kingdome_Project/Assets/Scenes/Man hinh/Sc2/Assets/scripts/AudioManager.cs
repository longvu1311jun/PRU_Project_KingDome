using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }
    public AudioSource musicAudioSource;
    public AudioSource vfxAudioSource;

    public AudioSource source;
    public AudioClip musicClip;
    public AudioClip coinClip;




    void Start()
    {
        musicAudioSource.clip = musicClip;
        musicAudioSource.Play();
    }

    public void PlaySFX(AudioClip sfxclip)
    {
        vfxAudioSource.clip = sfxclip;
        vfxAudioSource.PlayOneShot(sfxclip);
    }



    private void Awake()
    {
        instance = this;
        source = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip _sound)
    {
        source.PlayOneShot(_sound);
    }
    }
