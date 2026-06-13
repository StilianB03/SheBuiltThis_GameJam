using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class StartScreenNewInput : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("The exact name of the scene you want to load next.")]
    [SerializeField] private string sceneToLoad;

    private System.IDisposable inputListener;

    void OnEnable()
    {
        // Hooks into the global input system to catch any button press event
        inputListener = InputSystem.onAnyButtonPress.Call(control => GoToNextScene());
    }

    void OnDisable()
    {
        // Always dispose of the listener to prevent memory leaks
        inputListener?.Dispose();
    }

    private void GoToNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene To Load is empty! Please type the scene name in the Inspector.");
        }
    }
}