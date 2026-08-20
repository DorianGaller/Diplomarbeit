using UnityEngine.Events;

/// <summary>
/// Allgemeines Interactable fuer alles, was einfach ein Event feuert
/// (PC-Terminal, Tueren, Werkbank, ...). Radius pro Objekt frei im Inspector einstellbar.
/// </summary>
public class Interactable : InteractableBase
{
    public UnityEvent onInteract;

    protected override void OnInteractPressed() => onInteract?.Invoke();
}