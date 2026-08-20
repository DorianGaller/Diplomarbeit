using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("References")]
    [SerializeField] private Canvas fadeCanvas;
    [SerializeField] private RectTransform fadePanel;

    [Header("Settings")]
    public float transitionDuration = 0.4f;

    private float panelWidth;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (fadeCanvas != null)
        {
            RectTransform canvasRect = fadeCanvas.GetComponent<RectTransform>();
            panelWidth = canvasRect.rect.width;
        }

        if (fadePanel != null)
            fadePanel.anchoredPosition = new Vector2(-panelWidth, 0);   // Start: komplett links außerhalb, Bildschirm frei
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(DoTransition(sceneName));
    }

    IEnumerator DoTransition(string sceneName)
    {
        yield return StartCoroutine(SlidePanel(-panelWidth, 0f));   // reinschieben -> Bildschirm wird schwarz

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone)
            yield return null;

        yield return null;   // einen Frame warten, bis die neue Szene sauber initialisiert ist

        yield return StartCoroutine(SlidePanel(0f, panelWidth));   // rausschieben -> neue Szene wird sichtbar
    }

    IEnumerator SlidePanel(float from, float to)
    {
        if (fadePanel == null) yield break;

        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float x = Mathf.Lerp(from, to, t / transitionDuration);
            fadePanel.anchoredPosition = new Vector2(x, 0);
            yield return null;
        }

        fadePanel.anchoredPosition = new Vector2(to, 0);
    }
}