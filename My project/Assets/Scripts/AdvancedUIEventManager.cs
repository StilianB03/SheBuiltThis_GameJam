using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Required for Image
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(CanvasGroup))] // Failsafe for the main script object
public class AdvancedUIEventManager : MonoBehaviour
{
    [Header("1. Fade-to-Black Settings")]
    [Tooltip("Drag your black Image UI object here.")]
    [SerializeField] private Image blackOverlayImage;
    [Tooltip("Duration in seconds for the screen to become completely black.")]
    [SerializeField] private float backgroundFadeDuration = 2f;

    [Header("2. Text Settings")]
    [Tooltip("Drag your TextMeshPro GameObject here.")]
    [SerializeField] private TextMeshProUGUI textToDisplay;
    [Tooltip("Duration in seconds for the text to become fully visible.")]
    [SerializeField] private float textFadeDuration = 1f;

    [Header("Timing")]
    [Tooltip("If true, text fades in AFTER background is black. If false, both fade in simultaneously.")]
    [SerializeField] private bool sequenceTextAfterBackground = true;

    private CanvasGroup blackOverlayCanvasGroup;
    private CanvasGroup textCanvasGroup;
    private bool sequenceTriggered = false;

    void Awake()
    {
        // 1. Set up the Black Overlay
        if (blackOverlayImage != null)
        {
            // We use CanvasGroup on the Image because it's more reliable for fading
            blackOverlayCanvasGroup = blackOverlayImage.GetComponent<CanvasGroup>();
            if (blackOverlayCanvasGroup == null)
            {
                // Add the CanvasGroup if the user forgot
                blackOverlayCanvasGroup = blackOverlayImage.gameObject.AddComponent<CanvasGroup>();
            }

            // Start state: Transparent (alpha 0)
            blackOverlayCanvasGroup.alpha = 0f;
            // Ensure the black image doesn't block raycasts while it's transparent
            blackOverlayCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("AdvancedUIEventManager: Black Overlay Image reference missing.");
        }

        // 2. Set up the TextMeshPro
        if (textToDisplay != null)
        {
            textCanvasGroup = textToDisplay.GetComponent<CanvasGroup>();
            if (textCanvasGroup == null)
            {
                // Add the CanvasGroup to text if missing
                textCanvasGroup = textToDisplay.gameObject.AddComponent<CanvasGroup>();
            }

            // Start state: Invisible
            textCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogError("AdvancedUIEventManager: TextToDisplay reference missing.");
        }
    }

    /// <summary>
    /// Call this function from your external event (e.g., when the player reaches the goal)
    /// to trigger the fade sequence.
    /// </summary>
    public void TriggerEndingSequence()
    {
        // Prevent accidental double triggers
        if (sequenceTriggered) return;
        sequenceTriggered = true;

        // Block UI interactions now that the event started
        if (blackOverlayCanvasGroup != null)
        {
            blackOverlayCanvasGroup.blocksRaycasts = true;
        }

        StartCoroutine(ExecuteSequence());
    }

    private IEnumerator ExecuteSequence()
    {
        if (sequenceTextAfterBackground)
        {
            // SEQUENCE: Black first, then Text
            yield return StartCoroutine(FadeCanvasGroup(blackOverlayCanvasGroup, 0f, 1f, backgroundFadeDuration));
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeDuration));
        }
        else
        {
            // PARALLEL: Fade both together
            StartCoroutine(FadeCanvasGroup(blackOverlayCanvasGroup, 0f, 1f, backgroundFadeDuration));
            StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeDuration));

            // Wait for the longer fade before continuing the coroutine
            yield return new WaitForSeconds(Mathf.Max(backgroundFadeDuration, textFadeDuration));
        }

        Debug.Log("Ending Sequence Complete.");
    }

    // Generic coroutine that fades ANY CanvasGroup from a starting alpha to an ending alpha
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
            float lerpValue = counter / duration;
            // Use smoothstep for a softer "ease-in/ease-out" transition
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, lerpValue);
            yield return null;
        }

        // Ensure we hit the exact final value
        cg.alpha = endAlpha;
    }
}