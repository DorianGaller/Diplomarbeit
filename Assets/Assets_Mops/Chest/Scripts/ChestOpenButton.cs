using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Füge dieses Skript auf denselben Button, der bisher per Inspector
/// "ChestUI.LoadAndOpen" aufgerufen hat. Verkabelt den Klick zur Laufzeit
/// neu mit der tatsächlich lebenden ChestUI, statt sich auf die im Editor
/// gespeicherte Referenz zu verlassen (die durch DontDestroy zerstört werden kann).
/// </summary>
[RequireComponent(typeof(Button))]
public class ChestOpenButton : MonoBehaviour
{
    [Tooltip("Die Truhe, die dieser Button öffnen soll")]
    [SerializeField] private Chest targetChest;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        StartCoroutine(RewireNextFrame());
    }

    private IEnumerator RewireNextFrame()
    {
        yield return null;   // warten bis DontDestroy-Duplikate bereinigt sind

        GameObject canvas = GameObject.Find("InventoryCanvas");
        ChestUI chestUI = canvas != null ? canvas.GetComponentInChildren<ChestUI>(true) : null;

        if (chestUI == null)
        {
            Debug.LogError("ChestOpenButton: Keine ChestUI gefunden!");
            yield break;
        }

        if (targetChest == null)
        {
            Debug.LogError("ChestOpenButton: Kein Target Chest zugewiesen!");
            yield break;
        }

        // Alte, evtl. tote Editor-Verkabelung entfernen und frisch verbinden
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => chestUI.LoadAndOpen(targetChest));
    }
}