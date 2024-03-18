using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceSound : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;
    [SerializeField] private bool ambienceOn = true;

    private AudioSource audioSource;
    private float volume = 0.3f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    private void Update()
    {
        if (ambienceOn && !audioSource.isPlaying) {
            AmbienceAudioSet();
            audioSource.Play();

        } else if (!ambienceOn && audioSource.isPlaying) {
            audioSource.Stop();
        }
    }

    private void AmbienceAudioSet()
    {
        audioSource.clip = audioClipRefsSO.ambience[Random.Range(0, audioClipRefsSO.ambience.Length)];
        audioSource.volume = volume;
    }

    public void AmbienceVolume(float volume)
    {
        this.volume = volume;
        audioSource.volume = this.volume;
    }

    public float GetVolume()
    {
        return volume;
    }
}
