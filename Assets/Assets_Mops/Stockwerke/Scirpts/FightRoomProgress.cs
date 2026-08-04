using System.Collections.Generic;

public static class FightRoomProgress
{
    private static Dictionary<string, int> visitsPerFloor = new Dictionary<string, int>();

    /// <summary>
    /// Zählt einen Fight-Room-Besuch für das angegebene Stockwerk hoch
    /// und gibt zurück, der wievielte Besuch das ist (1 = erster Fight Room, 2 = zweiter, usw.)
    /// </summary>
    public static int RegisterFightRoomEntry(string floorID)
    {
        if (!visitsPerFloor.ContainsKey(floorID))
            visitsPerFloor[floorID] = 0;

        visitsPerFloor[floorID]++;
        return visitsPerFloor[floorID];
    }

    /// <summary>
    /// Setzt alle Zähler zurück — z.B. beim Start eines neuen Runs.
    /// </summary>
    public static void ResetAll()
    {
        visitsPerFloor.Clear();
    }
}