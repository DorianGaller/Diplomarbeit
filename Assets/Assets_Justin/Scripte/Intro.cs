using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    [SerializeField] private RawImage introImage;
    [SerializeField] private RenderTexture introTexture;
    [SerializeField] private string nextSceneName = "Press_Any_Button Screen";
    [SerializeField] private float introDuration = 11f;

    private float elapsedTime;

    private void Awake()
    {
        if (introImage == null)
        {
            introImage = GetComponent<RawImage>();
        }

        if (introImage != null && introTexture != null)
        {
            introImage.texture = introTexture;
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= introDuration || Input.anyKeyDown)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("No next scene name assigned for the intro.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
