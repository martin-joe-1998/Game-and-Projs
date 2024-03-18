using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RainSound : MonoBehaviour
{
    [SerializeField] private GameObject rainyWindowGlass;       // Window object of rain weather, false as default
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;   // Audio clip reference

    private AudioSource audioSource;
    private bool isRainActive;
    private float volume = 0.7f;      // Volume of rain sound

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Get if Rainy Window is Active
        isRainActive = rainyWindowGlass.activeSelf;
    }

    private void Update()
    {
        if (isRainActive && !audioSource.isPlaying) {
            RainAudioSet();
            audioSource.Play();
            
        } else if (!isRainActive && audioSource.isPlaying) {
            audioSource.Stop();
        }

        // Update Window state
        isRainActive = rainyWindowGlass.activeSelf;
    }

    private void RainAudioSet()
    {
        audioSource.clip = audioClipRefsSO.rain[Random.Range(0, audioClipRefsSO.rain.Length)];
        audioSource.volume = volume;
    }

    public void RainVolume(float volume)
    {
        this.volume = volume;
        audioSource.volume = this.volume;
    }

    public float GetVolume()
    {
        return volume;
    }
}
