using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField] private RainSound rainSound;
    [SerializeField] private AmbienceSound ambienceSound;
    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    [SerializeField] private Slider rainVolumeSlider;
    [SerializeField] private Slider ambienceVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    private float volume = 1f;
    private float rainVolume;
    private float ambienceVolume;
    private float musicVolume;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rainVolume = rainSound.GetVolume();
        ambienceVolume = ambienceSound.GetVolume();
        musicVolume = musicPlayer.GetVolume();
        SliderInitialize();
    }

    // ---------------------------------------------------------------------------------------------------

    // Play a audio clip for one time
    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f)
    {
        AudioSource.PlayClipAtPoint(audioClip, position, volume);
    }

    // Play a random audio clip in audioClipRefsSO for one time
    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    public void PlayFootstepsSound(Vector3 position, float volumeMultiplier)
    {
        PlaySound(audioClipRefsSO.footstep, position, volumeMultiplier * volume);
    }

    private void SliderInitialize()
    {
        rainVolumeSlider.value = rainVolume;
        ambienceVolumeSlider.value = ambienceVolume;
        musicVolumeSlider.value = musicVolume;
    }
}
