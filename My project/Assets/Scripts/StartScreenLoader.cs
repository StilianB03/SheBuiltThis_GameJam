using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class FlexibleSceneLoader : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("The exact name of the scene you want to load next.")]
    [SerializeField] private string sceneToLoad;

    [Header("Input Settings")]
    [Tooltip("If enabled, pressing ANY key/button will trigger the transition to the next scene. Disable this for your main game scene.")]
    [SerializeField] private bool loadOnAnyKeyPress = true;

    [Header("Transition References")]
    [Tooltip("The Animator component controlling the transition animations.")]
    [SerializeField] private Animator transition;

    [Header("Transition Settings")]
    [Tooltip("Time in seconds to wait for the start animation to finish before loading the scene.")]
    [SerializeField] private float transitionTime = 1f;

    private System.IDisposable inputListener;
    private bool isTransitioning = false;

    void OnEnable()
    {
        // Only hook into the global input system if the checkbox is checked
        if (loadOnAnyKeyPress)
        {
            inputListener = InputSystem.onAnyButtonPress.Call(control => HandleInputTrigger());
        }
    }

    void OnDisable()
    {
        // Clean up the listener to prevent memory leaks
        CleanupListener();
    }

    private void HandleInputTrigger()
    {
        // Guard clause to ensure we only trigger the transition sequence once
        if (isTransitioning) return;

        isTransitioning = true;
        CleanupListener();

        StartCoroutine(LoadLevelSequence());
    }

    /// <summary>
    /// Call this function manually (from a UI Button click event or an trigger script) 
    /// when 'loadOnAnyKeyPress' is turned off.
    /// </summary>
    public void TriggerManualTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(LoadLevelSequence());
    }

    private IEnumerator LoadLevelSequence()
    {
        // 1. Trigger the fade/wipe animation
        if (transition != null)
        {
            transition.SetTrigger("Start");
        }
        else
        {
            Debug.LogWarning("Transition Animator is missing! Loading scene without animation.");
        }

        // 2. Wait for the transition duration
        yield return new WaitForSeconds(transitionTime);

        // 3. Perform the actual scene swap
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene To Load is empty! Cannot switch scenes. Please assign a scene name in the Inspector.");
            isTransitioning = false; // Reset flag if it fails so you can try again
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