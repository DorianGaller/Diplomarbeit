using UnityEngine;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        // Singleton Pattern - nur eine Instanz erlaubt
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("FadeManager erstellt und persistent gemacht");
        }
        else
        {
            Debug.Log("FadeManager existiert bereits - diese Instanz wird zerstört");
            Destroy(gameObject);
        }
    }

    public CanvasGroup GetCanvasGroup()
    {
        return canvasGroup;
    }
}