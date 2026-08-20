using UnityEngine;
using TMPro;

/// <summary>
/// Singleton: einzige Stelle, die das geteilte Interact-Prompt-Textfeld schaltet.
/// Sitzt auf einem IMMER AKTIVEN Objekt (z.B. InfoTextPlayer), damit Awake()
/// garantiert laeuft - auch wenn InteractText selbst per Default deaktiviert
/// in der Szene liegt (z.B. weil der Player DontDestroyOnLoad hat und die UI
/// in andere Szenen mitnimmt, wo der Prompt zu Beginn nicht sichtbar sein soll).
/// promptTextMesh referenziert dabei nur das Kind-Objekt, wird aber selbst
/// beim Awake immer zwangsweise deaktiviert - unabhaengig vom Editor-Zustand.
/// </summary>
public class DGInteractPrompt : MonoBehaviour
{
    public static DGInteractPrompt Instance { get; private set; }

    [Tooltip("Das InteractText-GameObject (mit TMP_Text). Darf im Editor aktiv ODER inaktiv gespeichert sein - wird beim Start ueber Code erzwungen deaktiviert.")]
    [SerializeField] private TMP_Text promptTextMesh;

    void Awake()
    {
        Instance = this;
        if (promptTextMesh != null)
            promptTextMesh.gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        if (promptTextMesh == null) return;
        promptTextMesh.text = text;
        promptTextMesh.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (promptTextMesh == null) return;
        if (promptTextMesh.gameObject.activeSelf)
            promptTextMesh.gameObject.SetActive(false);
    }
}