using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Profil-Auswahl im Hauptmenue.
/// Leerer Slot -> neues Spiel, belegter Slot -> Spielstand laden.
/// Loeschen nur nach Bestaetigung.
/// </summary>
public class ProfileMenu : MonoBehaviour
{
    [System.Serializable]
    public class ProfileSlotUI
    {
        public Button playButton;
        public TMP_Text label;
        public Button deleteButton;
    }

    [Header("Slots (Reihenfolge = Profil 1, 2, 3)")]
    [SerializeField] private ProfileSlotUI[] slots = new ProfileSlotUI[SaveSystem.SlotCount];

    [Header("Neues Spiel")]
    [SerializeField] private string newGameScene = "Escape Room";
    [SerializeField] private string newGameSpawnId = "";

    [Header("Loeschen bestaetigen")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private int pendingDeleteSlot = -1;

    private void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;   // eigene Kopie fuer den Lambda

            if (slots[i].playButton != null)
                slots[i].playButton.onClick.AddListener(() => Play(index));

            if (slots[i].deleteButton != null)
                slots[i].deleteButton.onClick.AddListener(() => AskDelete(index));
        }

        if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ConfirmDelete);
        if (confirmNoButton != null) confirmNoButton.onClick.AddListener(CloseConfirm);

        CloseConfirm();
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SaveData save = SaveSystem.PeekProfile(i);

            if (slots[i].label != null)
                slots[i].label.text = save != null
                    ? $"Profile {i + 1}\n{save.sceneName}\n{save.lastSaved}"
                    : $"Profile {i + 1}\nNew Game";

            if (slots[i].deleteButton != null)
                slots[i].deleteButton.gameObject.SetActive(save != null);
        }
    }

    private void Play(int slot)
    {
        if (SaveSystem.HasSave(slot))
        {
            if (!SaveSystem.LoadProfile(slot))
                Debug.LogError("ProfileMenu: Profil " + (slot + 1) + " konnte nicht geladen werden.");
            return;
        }

        SaveSystem.StartNewGame(slot, newGameScene, newGameSpawnId);
    }

    private void AskDelete(int slot)
    {
        if (confirmPanel == null)
        {
            Debug.LogError("ProfileMenu: Kein Confirm Panel zugewiesen, Profil wird nicht geloescht.");
            return;
        }

        pendingDeleteSlot = slot;

        if (confirmText != null)
            confirmText.text = $"Are you sure you want to delete Profile {slot + 1}?\nThis cannot be undone.";

        confirmPanel.SetActive(true);
    }

    private void ConfirmDelete()
    {
        if (pendingDeleteSlot >= 0)
        {
            SaveSystem.DeleteProfile(pendingDeleteSlot);
            Refresh();
        }

        CloseConfirm();
    }

    private void CloseConfirm()
    {
        pendingDeleteSlot = -1;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}