using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : SingletonMono<SoundManager>
{
    public AudioClip globalsound;

    [Range(0, 2)]
    public float globalVolume;

    private AudioSource m_AudioSource;
    public void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        if (m_AudioSource == null)
        {
            m_AudioSource = gameObject.AddComponent<AudioSource>();
        }
        m_AudioSource.clip = globalsound;
    }

    public void PlayOneShotGlobalSound()
    {
        m_AudioSource.volume = globalVolume;
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
    }
}
