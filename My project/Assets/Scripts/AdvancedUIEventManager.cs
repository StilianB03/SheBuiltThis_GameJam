using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[RequireComponent(typeof(CanvasGroup))]
public class AdvancedUIEventManager : MonoBehaviour
{
    [Header("0. Pre-Fade Out (Current UI)")]
    [Tooltip("Drag the CanvasGroup containing your current gameplay HUD elements here to fade them out first.")]
    [SerializeField] private CanvasGroup gameplayUICanvasGroup;
    [SerializeField] private float gameplayFadeOutDuration = 0.5f;

    [Header("1. Fade-to-Black Settings")]
    [Tooltip("Drag your black screen UI GameObject here.")]
    [SerializeField] private GameObject blackOverlayObject;
    [Tooltip("Duration in seconds for the screen to become completely black.")]
    [SerializeField] private float backgroundFadeDuration = 2f;

    [Header("2. Text Settings")]
    [Tooltip("Drag your TextMeshPro UI GameObject here.")]
    [SerializeField] private GameObject textToDisplayObject;
    [Tooltip("Duration in seconds for the text to become fully visible.")]
    [SerializeField] private float textFadeDuration = 1f;

    [Header("Timing & Flow Setup")]
    [Tooltip("If true, text fades in AFTER background is black. If false, both fade in simultaneously.")]
    [SerializeField] private bool sequenceTextAfterBackground = true;
    [Tooltip("If enabled, pressing ANY key after visuals fade in will trigger the scene change.")]
    [SerializeField] private bool loadOnAnyKeyPress = true;
    [Tooltip("If enabled, this script will close the application instead of loading a scene.")]
    [SerializeField] private bool quitGame = false;

    [Header("Next Scene & Animator Settings")]
    [Tooltip("The exact name of the scene you want to load next.")]
    [SerializeField] private string sceneToLoad;
    [Tooltip("The Animator component controlling your final scene transition animations.")]
    [SerializeField] private Animator transitionAnimator;
    [Tooltip("Time in seconds to wait for the animator's 'Start' animation to finish before swapping scenes.")]
    [SerializeField] private float transitionTime = 1f;

    private CanvasGroup blackOverlayCanvasGroup;
    private CanvasGroup textCanvasGroup;
    private System.IDisposable inputListener;

    private bool sequenceTriggered = false;
    private bool visualsCompleted = false;
    private bool isSwappingScenes = false;

    void Awake()
    {
        // 1. Set up the Black Overlay (Identical to text logic)
        if (blackOverlayObject != null)
        {
            blackOverlayCanvasGroup = blackOverlayObject.GetComponent<CanvasGroup>();
            if (blackOverlayCanvasGroup == null)
            {
                blackOverlayCanvasGroup = blackOverlayObject.AddComponent<CanvasGroup>();
            }
            blackOverlayCanvasGroup.alpha = 0f;
            blackOverlayCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("AdvancedUIEventManager: Black Overlay GameObject reference missing.");
        }

        // 2. Set up the TextMeshPro GameObject
        if (textToDisplayObject != null)
        {
            textCanvasGroup = textToDisplayObject.GetComponent<CanvasGroup>();
            if (textCanvasGroup == null)
            {
                textCanvasGroup = textToDisplayObject.AddComponent<CanvasGroup>();
            }
            textCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogError("AdvancedUIEventManager: TextToDisplay GameObject reference missing.");
        }
    }

    void OnDisable()
    {
        CleanupListener();
    }

    /// <summary>
    /// Call this from BossController.cs to trigger the whole sequence.
    /// </summary>
    public void TriggerEndingSequence()
    {
        if (sequenceTriggered) return;
        sequenceTriggered = true;

        if (blackOverlayCanvasGroup != null)
        {
            blackOverlayCanvasGroup.blocksRaycasts = true;
        }

        StartCoroutine(ExecuteSequence());
    }

    private IEnumerator ExecuteSequence()
    {
        // STEP 0: Fade out existing gameplay UI elements first
        if (gameplayUICanvasGroup != null)
        {
            gameplayUICanvasGroup.blocksRaycasts = false;
            yield return StartCoroutine(FadeCanvasGroup(gameplayUICanvasGroup, 1f, 0f, gameplayFadeOutDuration));
        }

        // STEP 1 & 2: Fade in Black Screen and Text
        if (sequenceTextAfterBackground)
        {
            yield return StartCoroutine(FadeCanvasGroup(blackOverlayCanvasGroup, 0f, 1f, backgroundFadeDuration));
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeDuration));
        }
        else
        {
            StartCoroutine(FadeCanvasGroup(blackOverlayCanvasGroup, 0f, 1f, backgroundFadeDuration));
            StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeDuration));
            yield return new WaitForSeconds(Mathf.Max(backgroundFadeDuration, textFadeDuration));
        }

        visualsCompleted = true;
        Debug.Log("Visual transitions complete. Waiting for player input...");

        if (loadOnAnyKeyPress)
        {
            inputListener = InputSystem.onAnyButtonPress.Call(control => HandleInputTrigger());
        }
    }

    private void HandleInputTrigger()
    {
        if (!visualsCompleted || isSwappingScenes) return;

        isSwappingScenes = true;
        CleanupListener();

        StartCoroutine(LoadNextTarget());
    }

    public void TriggerManualSceneLoad()
    {
        HandleInputTrigger();
    }

    private IEnumerator LoadNextTarget()
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
        }

        if (quitGame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (!string.IsNullOrEmpty(sceneToLoad))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError("Scene To Load is empty! Assign a scene name in the Inspector.");
            isSwappingScenes = false;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        if (cg == null || duration <= 0)
        {
            if (cg != null) cg.alpha = endAlpha;
            yield break;
        }

        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, counter / duration);
            yield return null;
        }

        cg.alpha = endAlpha;
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