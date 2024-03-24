using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private MusicPlaylistSO musicPlaylistSO;
    [SerializeField] private Image playImage;
    [SerializeField] private Image pauseImage;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private AudioSource audioSource;
    private RectTransform textRectTransform;  // position of text
    private float volume = .6f;               // Music Volume
    private int currentTrackIndex = 0;        // Index of current playing music, 0 as initialized number.
    private string musicName;                 // Name of music
    private int playlistLength;               // Music Playlist Length
    private bool isPlaying;                   // if music is playing, update when initialized, "play" and "pause" button pressed
    private float scrollSpeed = 0.3f;         // Scroll Speed of music name text
    private float textSlideWidth;             // Width of text slide bar
    private Vector2 position;                 // Position of text(music name)

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        MusicPlayerInitialize();
    }

    private void Update()
    {
        // Make music name text slide from left to right
        MoveText();

        //if current song is over, play next song
        if (!audioSource.isPlaying && isPlaying) {
            PlayNext();
        }
    }

    public void PlayAndPauseMusic()
    {
        // When music is not playing, start playing music and change UI image
        if (!audioSource.isPlaying) {
            if (musicPlaylistSO != null && audioSource.clip != null) {
                audioSource.Play();
                isPlaying = true;
                playImage.enabled = false;
                pauseImage.enabled = true;
            } else {
                Debug.Log("Can't Play.");
            }
        }
        // When music is playing, pause music and change UI image
        else {
            audioSource.Pause();
            isPlaying = false;
            playImage.enabled = true;
            pauseImage.enabled = false;
        }
    }

    private void MusicPlayerInitialize()
    {
        playlistLength = musicPlaylistSO.Music.Length;                     // Get length of playlist to save computing resource
        audioSource.clip = musicPlaylistSO.Music[currentTrackIndex];       // Set first song as initialized music
        audioSource.volume = volume;                                       // Set volume
                                                                           
        musicName = audioSource.clip.name;                                 // Get name of current music
        textMeshProUGUI.text = musicName;                                  // Set music name to slide text
        textRectTransform = textMeshProUGUI.rectTransform;                 // Get slide text bar's transform
        textSlideWidth = textRectTransform.rect.width * textRectTransform.localScale.x;  // Get actual width of slide text bar
        position = textRectTransform.anchoredPosition;                     // Get slide text bar's position 

        playImage.enabled = true;           // Play Button Image enable (cause music is not playing initially)
        pauseImage.enabled = false;         // Pause Button Image disable

        isPlaying = audioSource.isPlaying;  // Set isPlaying as audio source's current state
    }

    public void PlayNext()
    {
        currentTrackIndex = ((currentTrackIndex + 1) % playlistLength) % playlistLength;

        UpdateNameAndIndex();
        if (isPlaying) {
            audioSource.Play();
        }
    }

    public void PlayPrevious()
    {
        currentTrackIndex = ((currentTrackIndex - 1) % playlistLength + playlistLength) % playlistLength;

        UpdateNameAndIndex();
        if (isPlaying) {
            audioSource.Play();
        }
    }

    public void MusicVolume(float volume)
    {
        this.volume = volume;
        audioSource.volume = this.volume;
    }

    public float GetVolume()
    {
        return volume;
    }

    private void UpdateNameAndIndex()
    {
        audioSource.clip = musicPlaylistSO.Music[currentTrackIndex];

        musicName = audioSource.clip.name;
        textMeshProUGUI.text = musicName;

        position.x = textSlideWidth;
    }

    // Make the Text slide from left to right
    void MoveText()
    {
        //position = textRectTransform.anchoredPosition;

        position.x -= scrollSpeed * Time.deltaTime;

        // When text slide out of range, back to the left side
        if (position.x < -textSlideWidth) {
            position.x = textSlideWidth;
        }

        textRectTransform.anchoredPosition = position;
    }
}
