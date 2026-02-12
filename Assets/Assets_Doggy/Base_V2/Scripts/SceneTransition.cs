using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad;
    
    private bool isTransitioning = false;

    private void Start()
    {
        Debug.Log("SceneTransition Script gestartet!");
        
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            Debug.Log("CanvasGroup gefunden und Alpha auf 0 gesetzt");
        }
        else
        {
            Debug.LogError("FEHLER: CanvasGroup ist nicht zugewiesen!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger betreten von: " + other.gameObject.name + " mit Tag: " + other.tag);
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player erkannt! Starte Transition...");
            
            if (!isTransitioning)
            {
                if (string.IsNullOrEmpty(sceneToLoad))
                {
                    Debug.LogError("FEHLER: Kein Scene-Name angegeben!");
                    return;
                }
                
                StartCoroutine(TransitionToScene());
            }
            else
            {
                Debug.Log("Transition läuft bereits...");
            }
        }
        else
        {
            Debug.Log("Kein Player - Tag stimmt nicht überein!");
        }
    }

    private IEnumerator TransitionToScene()
    {
        isTransitioning = true;
        Debug.Log("Fade to Black startet...");

        // Fade to Black
        yield return StartCoroutine(Fade(1f));
        
        Debug.Log("Lade Szene: " + sceneToLoad);

        // Lade die neue Szene
        SceneManager.LoadScene(sceneToLoad);

        // Fade from Black (wird in der neuen Szene ausgeführt)
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("FEHLER: fadeCanvasGroup ist null!");
            yield break;
        }

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        Debug.Log("Fade abgeschlossen. Alpha ist jetzt: " + targetAlpha);
    }
}


