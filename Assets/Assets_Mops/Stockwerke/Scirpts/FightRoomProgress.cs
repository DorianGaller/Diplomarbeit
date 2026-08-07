using System.Collections.Generic;

public static class FightRoomProgress
{
    private static Dictionary<string, int> elevatorVisitsPerFloor = new Dictionary<string, int>();

    /// <summary>Wird NUR vom Elevator aufgerufen, wenn ein Stockwerk neu betreten wird.</summary>
    public static int RegisterElevatorEntry(string floorID)
    {
        if (!elevatorVisitsPerFloor.ContainsKey(floorID))
            elevatorVisitsPerFloor[floorID] = 0;

        elevatorVisitsPerFloor[floorID]++;
        return elevatorVisitsPerFloor[floorID];
    }

    /// <summary>Liest den aktuellen Besuchszähler, OHNE ihn zu erhöhen.</summary>
    public static int GetVisitCount(string floorID)
    {
        return elevatorVisitsPerFloor.TryGetValue(floorID, out int count) ? count : 0;
    }

    public static void ResetAll()
    {
        elevatorVisitsPerFloor.Clear();
    }
}