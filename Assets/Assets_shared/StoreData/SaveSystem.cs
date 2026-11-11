using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Spielstand-System mit mehreren Profilen.
///
/// - Speichert automatisch, sobald ein Raum ueber den SceneTransitionManager verlassen wird.
/// - Dateien liegen in Application.persistentDataPath (profile_1.json usw.),
///   das funktioniert auch im fertigen Build.
/// - Ohne gewaehltes Profil (Szene direkt im Editor gestartet) wird nichts auf die
///   Festplatte geschrieben. Raum-Fortschritt gilt dann nur bis zum Beenden.
///
/// SETUP: keines. Legt sich beim Spielstart selbst an.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public const int SlotCount = 3;

    private static SaveSystem instance;
    private static SaveData data = new SaveData();
    private static bool applyPlayerPending;

    /// <summary>Aktives Profil (0 bis SlotCount-1), -1 = keins gewaehlt.</summary>
    public static int CurrentSlot { get; private set; } = -1;

    public static bool StoryCompleted => data.storyCompleted;

    // ── Start ─────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        data = new SaveData();
        CurrentSlot = -1;
        applyPlayerPending = false;

        GameObject go = new GameObject("SaveSystem");
        instance = go.AddComponent<SaveSystem>();
        DontDestroyOnLoad(go);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Laeuft nach Awake, aber vor Start der Objekte in der neuen Szene
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyChests(scene);

        if (applyPlayerPending && instance != null)
            instance.StartCoroutine(ApplyPlayerNextFrames());
    }

    // ── Profile ───────────────────────────────────────────

    private static string GetPath(int slot) =>
        Path.Combine(Application.persistentDataPath, "profile_" + (slot + 1) + ".json");

    public static bool HasSave(int slot) => File.Exists(GetPath(slot));

    /// <summary>Liest ein Profil, ohne es zu laden (fuer die Anzeige im Menue).</summary>
    public static SaveData PeekProfile(int slot)
    {
        if (!HasSave(slot)) return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(GetPath(slot)));
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Profil " + (slot + 1) + " ist beschaedigt: " + e.Message);
            return null;
        }
    }

    public static void StartNewGame(int slot, string sceneName, string spawnId = "")
    {
        CurrentSlot = slot;
        data = new SaveData { sceneName = sceneName, spawnId = spawnId };
        BeginSession();
        WriteFile();
        LoadScene(sceneName, spawnId);
    }

    public static bool LoadProfile(int slot)
    {
        SaveData loaded = PeekProfile(slot);
        if (loaded == null || string.IsNullOrEmpty(loaded.sceneName)) return false;

        CurrentSlot = slot;
        data = loaded;
        BeginSession();
        LoadScene(data.sceneName, data.spawnId);
        return true;
    }

    public static void DeleteProfile(int slot)
    {
        if (HasSave(slot))
            File.Delete(GetPath(slot));

        if (CurrentSlot == slot)
            CurrentSlot = -1;
    }

    private static void BeginSession()
    {
        applyPlayerPending = true;
        TutorialManager.ResetSession();

        FightRoomProgress.ResetAll();
        foreach (FloorVisitSave visit in data.floorVisits)
            FightRoomProgress.SetVisitCount(visit.floorID, visit.count);
    }

    // Absichtlich ohne SceneTransitionManager, sonst wuerde beim Laden sofort gespeichert
    private static void LoadScene(string sceneName, string spawnId)
    {
        DoorTransition.nextSpawnID = spawnId ?? "";
        SceneManager.LoadScene(sceneName);
    }

    // ── Speichern ─────────────────────────────────────────

    /// <summary>Wird vom SceneTransitionManager bei jedem Raumwechsel aufgerufen.</summary>
    public static void SaveOnRoomExit(string targetScene, string spawnId)
    {
        if (applyPlayerPending) return;

        CaptureChests(SceneManager.GetActiveScene());
        CapturePlayer();

        data.sceneName = targetScene;
        data.spawnId = spawnId;

        data.floorVisits.Clear();
        foreach (KeyValuePair<string, int> visit in FightRoomProgress.All)
            data.floorVisits.Add(new FloorVisitSave { floorID = visit.Key, count = visit.Value });

        WriteFile();
    }

    private static void WriteFile()
    {
        if (CurrentSlot < 0) return;

        data.lastSaved = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        try
        {
            // Erst in eine Temp-Datei, damit ein Absturz beim Schreiben das Profil nicht zerstoert
            string path = GetPath(CurrentSlot);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            File.Copy(tmp, path, true);
            File.Delete(tmp);
        }
        catch (System.Exception e)
        {
            Debug.LogError("SaveSystem: Speichern fehlgeschlagen: " + e.Message);
        }
    }

    // ── Spieler ───────────────────────────────────────────

    private static void CapturePlayer()
    {
        InventoryManager inv = FindInventory();
        if (inv == null) return;   // z.B. im Hauptmenue gibt es keinen Spieler

        data.items = CaptureSlots(inv.itemSlot);
        data.equipment = CaptureSlots(inv.equipmentSlot);
        data.equipped = CaptureSlots(inv.equippedSlot);
        data.weapons = new List<WeaponInstance>(WeaponRegistry.All);

        PlayerStats stats = FindStats();
        if (stats != null)
            data.coins = stats.coins;
        
        PlayerLife life = FindPlayerLife();
        if (life != null)
        {
            data.baseMaxHP = life.baseMaxHP;
            data.currentHP = life.CurrentHP;
        }
    }

    private static List<SlotSave> CaptureSlots(IDraggableSlot[] slots)
    {
        List<SlotSave> list = new List<SlotSave>();
        if (slots == null) return list;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || !slots[i].HasItem()) continue;

            list.Add(new SlotSave
            {
                slotIndex = i,
                itemName = slots[i].GetItemName(),
                quantity = slots[i].GetQuantity(),
                itemDescription = slots[i].GetItemDescription(),
                itemType = slots[i].GetItemType(),
                instanceId = slots[i].GetInstanceId()
            });
        }
        return list;
    }

    private static IEnumerator ApplyPlayerNextFrames()
    {
        // Warten, bis DontDestroy-Duplikate weg sind und alle Slots ihr Start() hatten
        yield return null;
        yield return null;

        applyPlayerPending = false;
        ApplyPlayer();
    }

    private static void ApplyPlayer()
    {
        InventoryManager inv = FindInventory();
        if (inv == null)
        {
            Debug.LogError("SaveSystem: InventoryCanvas nicht gefunden, Spielstand wurde nicht angewendet!");
            return;
        }

        // Erst alles leeren. Ausgeruestetes zuerst, sonst legt es sich zurueck ins Inventar.
        ClearSlots(inv.equippedSlot);
        ClearSlots(inv.itemSlot);
        ClearSlots(inv.equipmentSlot);

        // Waffen-Fortschritt vor den Slots, damit die instanceIds gleich passen
        WeaponRegistry.ReplaceAll(data.weapons);

        RestoreSlots(inv, inv.itemSlot, data.items);
        RestoreSlots(inv, inv.equipmentSlot, data.equipment);
        RestoreSlots(inv, inv.equippedSlot, data.equipped);

        PlayerStats stats = FindStats();
        if (stats != null)
        {
            stats.coins = data.coins;
            stats.UpdateCoinsDisplay();
        }

        PlayerLife life = FindPlayerLife();
        if (life != null && data.baseMaxHP > 0 && data.currentHP > 0)
            life.LoadHealth(data.baseMaxHP, data.currentHP);

        inv.DeselectAllSlots();
    }

    private static void ClearSlots(IDraggableSlot[] slots)
    {
        if (slots == null) return;

        foreach (IDraggableSlot slot in slots)
            if (slot != null && slot.HasItem())
                slot.ClearSlotData();
    }

    private static void RestoreSlots(InventoryManager inv, IDraggableSlot[] slots, List<SlotSave> saved)
    {
        if (slots == null || saved == null) return;

        foreach (SlotSave s in saved)
        {
            if (s.slotIndex < 0 || s.slotIndex >= slots.Length || slots[s.slotIndex] == null) continue;

            slots[s.slotIndex].SetSlotData(s.itemName, s.quantity, FindSprite(inv, s.itemName),
                                           s.itemDescription, s.itemType, s.instanceId);
        }
    }

    // Sprites lassen sich nicht speichern -> ueber den Namen wieder suchen
    private static Sprite FindSprite(InventoryManager inv, string itemName)
    {
        Sprite sprite = ItemDatabase.GetSprite(itemName);
        if (sprite != null) return sprite;

        if (inv.itemSOs != null)
            foreach (ItemSO so in inv.itemSOs)
                if (so != null && so.itemName == itemName && so.itemSprite != null)
                    return so.itemSprite;

        Debug.LogWarning("SaveSystem: Kein Sprite fuer '" + itemName + "' gefunden. Bitte in die ItemDatabase eintragen.");
        return null;
    }

    private static InventoryManager FindInventory()
    {
        GameObject canvas = GameObject.Find("InventoryCanvas");
        return canvas != null ? canvas.GetComponent<InventoryManager>() : null;
    }

    private static PlayerStats FindStats()
    {
        
        GameObject statManager = GameObject.Find("StatManager");
        return statManager != null ? statManager.GetComponent<PlayerStats>() : null;
    }

    private static PlayerLife FindPlayerLife()
    {
        GameObject player = GameObject.FindWithTag("Player");
        return player != null ? player.GetComponent<PlayerLife>() : null;
    }

    // ── Kisten ────────────────────────────────────────────

    private static void CaptureChests(Scene scene)
    {
        foreach (Chest chest in FindObjectsByType<Chest>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (chest.gameObject.scene != scene || chest.chestItems == null) continue;

            string id = GetObjectId(chest.gameObject);
            ChestSave save = data.chests.Find(c => c.id == id);
            if (save == null)
            {
                save = new ChestSave { id = id };
                data.chests.Add(save);
            }

            save.quantities.Clear();
            foreach (Chest.ChestItem item in chest.chestItems)
                save.quantities.Add(item != null ? item.quantity : 0);
        }
    }

    private static void ApplyChests(Scene scene)
    {
        foreach (Chest chest in FindObjectsByType<Chest>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (chest.gameObject.scene != scene || chest.chestItems == null) continue;

            string id = GetObjectId(chest.gameObject);
            ChestSave save = data.chests.Find(c => c.id == id);

            // Kisteninhalt im Inspector geaendert -> alten Stand verwerfen
            if (save == null || save.quantities.Count != chest.chestItems.Length) continue;

            for (int i = 0; i < chest.chestItems.Length; i++)
                if (chest.chestItems[i] != null)
                    chest.chestItems[i].quantity = save.quantities[i];
        }
    }

    // ── Welt-Fortschritt ──────────────────────────────────

    /// <summary>Eindeutige ID eines Objekts: Szenenpfad + Hierarchie (mit Geschwister-Index).</summary>
    public static string GetObjectId(GameObject go)
    {
        StringBuilder sb = new StringBuilder();
        for (Transform t = go.transform; t != null; t = t.parent)
            sb.Insert(0, "/" + t.name + "#" + t.GetSiblingIndex());

        return go.scene.path + sb;
    }

    public static bool IsCleared(string id) => data.clearedObjects.Contains(id);

    public static void MarkCleared(string id)
    {
        if (!data.clearedObjects.Contains(id))
            data.clearedObjects.Add(id);
    }

    public static bool IsLevelCompleted(string levelId) =>
        !string.IsNullOrEmpty(levelId) && data.completedLevels.Contains(levelId);

        public static bool IsFloorUnlocked(string floorId) =>
        !string.IsNullOrEmpty(floorId) && data.unlockedFloors.Contains(floorId);

    /// <summary>Wird beim Betreten eines Boss-Exits aufgerufen (TouchSceneTransition).</summary>
    public static void UnlockFloor(string floorId)
    {
        if (string.IsNullOrEmpty(floorId) || data.unlockedFloors.Contains(floorId)) return;
        data.unlockedFloors.Add(floorId);
    }

    /// <summary>Wird vom Boss beim Tod aufgerufen.</summary>
    public static void MarkBossDefeated(GameObject boss, bool isFinalBoss)
    {
        MarkCleared(GetObjectId(boss));

        string levelId = GetLevelId(boss.scene.path);
        if (!string.IsNullOrEmpty(levelId) && !data.completedLevels.Contains(levelId))
            data.completedLevels.Add(levelId);

        if (isFinalBoss)
            data.storyCompleted = true;
    }

    /// <summary>
    /// Wird vom Elevator aufgerufen. Ist die Story fertig und das Ziel-Level schon geschafft,
    /// wird es fuer den Replay zurueckgesetzt (Fight Rooms, Kisten, Boss, Tueren frisch).
    /// </summary>
    public static void OnElevatorEntry(string sceneName)
    {
        if (!data.storyCompleted) return;

        string levelId = GetLevelId(GetScenePath(sceneName));
        if (!IsLevelCompleted(levelId)) return;

        data.clearedObjects.RemoveAll(id => GetLevelId(id) == levelId);
        data.chests.RemoveAll(c => GetLevelId(c.id) == levelId);
    }

    /// <summary>Level = Ordner der Szene, z.B. ".../Stockwerke/1. Stock/Fight Room.unity" -> "1. Stock".</summary>
    public static string GetLevelId(string scenePathOrId)
    {
        if (string.IsNullOrEmpty(scenePathOrId)) return null;

        int end = scenePathOrId.IndexOf(".unity");
        string scenePath = end >= 0 ? scenePathOrId.Substring(0, end + 6) : scenePathOrId;
        return Path.GetFileName(Path.GetDirectoryName(scenePath));
    }

    private static string GetScenePath(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
                return path;
        }
        return null;
    }
}