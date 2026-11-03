using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Profil-Auswahl im Hauptmenue.
/// Leerer Slot -> neues Spiel, belegter Slot -> Spielstand laden.
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

    private void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;   // eigene Kopie fuer den Lambda

            if (slots[i].playButton != null)
                slots[i].playButton.onClick.AddListener(() => Play(index));

            if (slots[i].deleteButton != null)
                slots[i].deleteButton.onClick.AddListener(() => Delete(index));
        }

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

    private void Delete(int slot)
    {
        SaveSystem.DeleteProfile(slot);
        Refresh();
    }
}