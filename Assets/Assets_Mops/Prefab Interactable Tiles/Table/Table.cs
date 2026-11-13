using UnityEngine;

/// <summary>
/// Tisch mit eigenem Inventar. Kann alles, was eine Chest kann
/// (Items im Inspector, mit F oeffnen, Interaction Hint).
/// Zusaetzlich: schliesst automatisch, wenn der Spieler zu weit weg ist.
/// </summary>
public class Table : Chest
{
    [Header("Table")]
    [SerializeField] private float closeRange = 3f;

    protected override void Update()
    {
        base.Update();

        if (player == null || !isOpen) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > closeRange)
            CloseChest();
    }
}