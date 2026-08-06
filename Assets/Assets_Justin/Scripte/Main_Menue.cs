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

    [Tooltip("Optional: set a specific scene name to return to when pressing Esc.")]
    public string previousSceneName;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadPreviousScene();
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

    public void LoadPreviousScene()
    {
        if (!string.IsNullOrEmpty(previousSceneName))
        {
            SceneManager.LoadScene(previousSceneName, LoadSceneMode.Single);
            return;
        }

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex = currentSceneIndex - 1;

        if (previousSceneIndex >= 0)
        {
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Debug.LogWarning("No previous scene is available to load.");
        }
    }
}
