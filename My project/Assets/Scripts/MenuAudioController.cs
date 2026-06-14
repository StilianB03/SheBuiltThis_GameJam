using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Included in case you want to change scenes next

public class StartScreenAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("AudioSource playing the looping background noise/ambient track.")]
    [SerializeField] private AudioSource bgmSource;

    [Tooltip("AudioSource used to play the transition feedback sound.")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip proceedSFXClip;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Scene Transition (Optional)")]
    [SerializeField] private string nextSceneName = "GameScene";

    private bool isTransitioning = false;

    void Start()
    {
        // Make sure your ambient background noise is rolling
        if (bgmSource && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    void Update()
    {
        // 1. Check if the player pressed ANY keyboard key or mouse click
        // 2. Ensure we haven't already started the transition
        if (!isTransitioning && Input.anyKeyDown)
        {
            HandleStartPress();
        }
    }

    private void HandleStartPress()
    {
        // Lock this down immediately so hitting multiple keys doesn't breaking the fade
        isTransitioning = true;

        // Play the punchy transition sound effect
        if (sfxSource && proceedSFXClip)
        {
            sfxSource.PlayOneShot(proceedSFXClip);
        }

        // Start melting away the background audio
        if (bgmSource)
        {
            StartCoroutine(FadeOutBGM());
        }
    }

    private IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Smoothly drop the volume over your set duration
            bgmSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }

        bgmSource.volume = 0;
        bgmSource.Stop();

        // Optional: Automatically trigger the next scene once everything is quiet
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}