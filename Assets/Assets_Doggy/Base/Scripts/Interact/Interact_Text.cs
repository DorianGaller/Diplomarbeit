using UnityEngine;
using TMPro;

/// <summary>
/// Singleton: verwaltet das EINE geteilte Interact-Prompt-Textfeld im Player-UI.
/// Kommt direkt auf das InteractText-GameObject. Interactables rufen nur noch
/// Show()/Hide() auf statt selbst SetActive zu machen - so gibt es keine Konflikte
/// mehr, wenn mehrere Interactables dasselbe UI-Element nutzen.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class PlayerInteractPrompt : MonoBehaviour
{
    public static PlayerInteractPrompt Instance { get; private set; }

    private TMP_Text promptTextMesh;

    void Awake()
    {
        Instance = this;
        promptTextMesh = GetComponent<TMP_Text>();
        gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        promptTextMesh.text = text;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}