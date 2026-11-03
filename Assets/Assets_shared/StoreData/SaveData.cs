using System.Collections.Generic;

/// <summary>
/// Alles, was in einem Profil gespeichert wird.
/// Nur einfache Felder und Listen, damit JsonUtility es direkt speichern kann.
/// </summary>
[System.Serializable]
public class SaveData
{
    public int version = 1;

    // Wo es weitergeht
    public string sceneName;
    public string spawnId;
    public string lastSaved;

    // Spieler
    public int coins;
    public List<SlotSave> items = new List<SlotSave>();
    public List<SlotSave> equipment = new List<SlotSave>();
    public List<SlotSave> equipped = new List<SlotSave>();
    public List<WeaponInstance> weapons = new List<WeaponInstance>();

    // Welt
    public List<string> clearedObjects = new List<string>();   // Fight Rooms, Bosse, Keycard-Tueren
    public List<ChestSave> chests = new List<ChestSave>();
    public List<FloorVisitSave> floorVisits = new List<FloorVisitSave>();

    // Story
    public List<string> completedLevels = new List<string>();
    public bool storyCompleted;
}

[System.Serializable]
public class SlotSave
{
    public int slotIndex;
    public string itemName;
    public int quantity;
    public string itemDescription;
    public ItemType itemType;
    public string instanceId;
}

[System.Serializable]
public class ChestSave
{
    public string id;
    public List<int> quantities = new List<int>();
}

[System.Serializable]
public class FloorVisitSave
{
    public string floorID;
    public int count;
}