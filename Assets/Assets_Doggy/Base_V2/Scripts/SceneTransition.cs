using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float blackScreenDuration = 0.2f;
    
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    
    private bool isTransitioning = false;
    private CanvasGroup fadeCanvasGroup;

    private void Start()
    {
        // Hole die CanvasGroup vom FadeManager
        if (FadeManager.Instance != null)
        {
            fadeCanvasGroup = FadeManager.Instance.GetCanvasGroup();
            
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
                Debug.Log("CanvasGroup vom FadeManager erhalten");
            }
            else
            {
                Debug.LogError("CanvasGroup ist null!");
            }
        }
        else
        {
            Debug.LogError("FadeManager nicht gefunden! Stelle sicher, dass das Canvas das FadeManager Script hat.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("Kein Scene-Name angegeben!");
                return;
            }
            
            StartCoroutine(TransitionToScene());
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        
        // Blockiere Inputs während der Transition
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
        }

        // Fade to Black
        yield return StartCoroutine(Fade(1f));
        
        // Halte den schwarzen Screen
        yield return new WaitForSeconds(blackScreenDuration);
        
        // Lade die neue Szene
        SceneManager.LoadScene(sceneToLoad);
        
        // Warte einen Frame
        yield return null;
        
        // Hole CanvasGroup wieder (wichtig nach Scene Load!)
        if (FadeManager.Instance != null)
        {
            fadeCanvasGroup = FadeManager.Instance.GetCanvasGroup();
        }

        // Fade from Black
        yield return StartCoroutine(Fade(0f));
        
        // Erlaube wieder Inputs
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            
            // Smoothstep für weicheren Fade
            t = t * t * (3f - 2f * t);
            
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
