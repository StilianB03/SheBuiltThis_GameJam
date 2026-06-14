using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class StartScreenLoader : MonoBehaviour
{
    [Header("Mode Settings")]
    [Tooltip("If enabled, this script will close the application instead of loading a scene. Perfect for End/Game Over scenes.")]
    [SerializeField] private bool quitGame = false;

    [Header("Scene Setup")]
    [Tooltip("The exact name of the scene you want to load next (Ignored if 'Quit Game' is checked).")]
    [SerializeField] private string sceneToLoad;

    [Header("Input Settings")]
    [Tooltip("If enabled, pressing ANY key/button will trigger the transition. Disable this for your main game scene.")]
    [SerializeField] private bool loadOnAnyKeyPress = true;

    [Header("Transition References")]
    [Tooltip("The Animator component controlling the transition animations.")]
    [SerializeField] private Animator transition;

    [Header("Transition Settings")]
    [Tooltip("Time in seconds to wait for the start animation to finish before loading/quitting.")]
    [SerializeField] private float transitionTime = 1f;

    [Header("Audio Settings")]
    [Tooltip("AudioSource playing the looping background noise.")]
    [SerializeField] private AudioSource bgmSource;

    [Tooltip("AudioSource used to play the button/key click SFX.")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("The sound effect that plays the moment a key is pressed.")]
    [SerializeField] private AudioClip proceedSFXClip;

    private System.IDisposable inputListener;
    private bool isTransitioning = false;

    void Start()
    {
        // Ensure background music/noise is actively playing on start
        if (bgmSource && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    void OnEnable()
    {
        if (loadOnAnyKeyPress)
        {
            inputListener = InputSystem.onAnyButtonPress.Call(control => HandleInputTrigger());
        }
    }

    void OnDisable()
    {
        CleanupListener();
    }

    private void HandleInputTrigger()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        CleanupListener();

        StartCoroutine(LoadLevelSequence());
    }

    public void TriggerManualTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(LoadLevelSequence());
    }

    private IEnumerator LoadLevelSequence()
    {
        // 1. Play the crisp audio feedback instantly
        if (sfxSource && proceedSFXClip)
        {
            sfxSource.PlayOneShot(proceedSFXClip);
        }

        // 2. Trigger the fade/wipe animation
        if (transition != null)
        {
            transition.SetTrigger("Start");
        }
        else
        {
            Debug.LogWarning("Transition Animator is missing! Proceeding without animation.");
        }

        // 3. Simultaneously wait for the animator AND fade out the BGM
        float startVolume = bgmSource != null ? bgmSource.volume : 0f;
        float currentTime = 0;

        while (currentTime < transitionTime)
        {
            currentTime += Time.deltaTime;
            if (bgmSource != null)
            {
                // Smoothly lower volume based on your transitionTime progress
                bgmSource.volume = Mathf.Lerp(startVolume, 0, currentTime / transitionTime);
            }
            yield return null;
        }

        // Clean stop once completely silent
        if (bgmSource != null)
        {
            bgmSource.volume = 0;
            bgmSource.Stop();
        }

        // 4. Perform the actual scene swap OR quit the game
        if (quitGame)
        {
            Debug.Log("Quit Game triggered.");
#if UNITY_EDITOR
            // This stops the play mode inside the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // This closes the actual built game (.exe / .app)
            Application.Quit();
#endif
        }
        else if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Start loading the scene in the background asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);

            // Wait until the asynchronous scene fully finishes loading
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError("Scene To Load is empty! Cannot switch scenes. Please assign a scene name in the Inspector.");
            isTransitioning = false;
        }
    }

    private void CleanupListener()
    {
        if (inputListener != null)
        {
            inputListener.Dispose();
            inputListener = null;
        }
    }
}