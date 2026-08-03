using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main_Menue : MonoBehaviour
{
    [Header("Button Targets")]
    public Button quitButton;
    public Button loadSceneButton;

    [Header("Scene Settings")]
    [Tooltip("Enter the exact scene name to load in the Inspector.")]
    public string selectedSceneName;

    void Start()
    {
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (loadSceneButton != null)
        {
            loadSceneButton.onClick.AddListener(LoadSelectedScene);
        }
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void LoadSelectedScene()
    {
        if (string.IsNullOrEmpty(selectedSceneName))
        {
            Debug.LogWarning("Please set a scene name in the Inspector before loading.");
            return;
        }

        SceneManager.LoadScene(selectedSceneName, LoadSceneMode.Single);
    }
}
