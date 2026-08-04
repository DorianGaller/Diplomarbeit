using UnityEngine;
using TMPro;

public class RoomModifierManager : MonoBehaviour
{
    [Header("Floor Identifikation")]
    [Tooltip("Eindeutige ID für dieses Stockwerk, z.B. '1.Stock', '2.Stock'")]
    public string floorID = "1.Stock";

    [Header("Zugehöriger Fight Room")]
    [Tooltip("Der EnemySpawn DIESES Raumbereichs — wichtig bei mehreren Fight Rooms in derselben Szene!")]
    [SerializeField] private EnemySpawn linkedEnemySpawn;

    [Header("Verfügbare Modifikatoren")]
    public RoomModifierSO[] possibleModifiers;

    [Range(0f, 1f)]
    public float noModifierChance = 0.3f;

    [Header("UI (optional)")]
    public TMP_Text modifierAnnounceText;
    public float announceDuration = 3f;

    private RoomModifierSO activeModifier;
    private bool roomActivated = false;   // NEU

    // NEU: wird vom FightRoomTrigger aufgerufen statt automatisch in Start()
    public void ActivateRoom()
    {
        if (roomActivated) return;
        roomActivated = true;

        int visitCount = FightRoomProgress.RegisterFightRoomEntry(floorID);

        if (linkedEnemySpawn != null)
            linkedEnemySpawn.OnAllWavesCompleted += RemoveActiveModifier;

        if (visitCount >= 2)
        {
            PickAndApplyModifier();
        }
        else
        {
            Debug.Log($"Fight Room #{visitCount} auf {floorID} — noch kein Modifikator (erst ab dem 2. Mal).");
        }
    }

    void PickAndApplyModifier()
    {
        if (possibleModifiers == null || possibleModifiers.Length == 0) return;

        if (Random.value <= noModifierChance)
        {
            Debug.Log("Kein Raum-Modifikator diese Runde.");
            return;
        }

        activeModifier = possibleModifiers[Random.Range(0, possibleModifiers.Length)];
        activeModifier.Apply();

        Debug.Log("Aktiver Raum-Modifikator: " + activeModifier.modifierName);

        if (modifierAnnounceText != null)
        {
            modifierAnnounceText.text = activeModifier.modifierName + "\n" + activeModifier.description;
            modifierAnnounceText.gameObject.SetActive(true);
            Invoke(nameof(HideAnnounce), announceDuration);
        }
    }

    void HideAnnounce()
    {
        if (modifierAnnounceText != null)
            modifierAnnounceText.gameObject.SetActive(false);
    }

    void RemoveActiveModifier()
    {
        if (activeModifier != null)
            activeModifier.Remove();
    }
}