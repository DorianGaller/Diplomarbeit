using UnityEngine;
using System.Collections;

public class WireInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string requiredItemName = "Scissors";
    [SerializeField] private float interactionRange = 2f;

    [Header("References")]
    [SerializeField] private EquippedSlot mainHandSlot;
    [Tooltip("Erscheint, sobald der Spieler in der Naehe ist (z.B. 'Schere ausruesten + Linksklick')")]
    [SerializeField] private GameObject interactionHint;

    private Transform player;
    private Camera cam;
    private Collider2D[] clickColliders;
    private Renderer[] renderers;

    private void Start()
    {
        clickColliders = GetComponentsInChildren<Collider2D>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
        StartCoroutine(InitNextFrame());
    }

    private IEnumerator InitNextFrame()
    {
        yield return null;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("WireInteraction: Kein GameObject mit Tag 'Player' gefunden!");

        if (mainHandSlot == null)
        {
            EquippedSlot[] allSlots = FindObjectsByType<EquippedSlot>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            var itemTypeField = typeof(EquippedSlot).GetField(
                "itemType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            foreach (EquippedSlot slot in allSlots)
            {
                if (itemTypeField != null &&
                    itemTypeField.GetValue(slot) is ItemType type && type == ItemType.mainHand)
                {
                    mainHandSlot = slot;
                    break;
                }
            }

            if (mainHandSlot == null)
                Debug.LogError("WireInteraction: MainHand EquippedSlot nicht gefunden! Bitte im Inspector zuweisen.");
        }

        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        bool playerInRange = Vector2.Distance(transform.position, player.position) <= interactionRange;

        // Hinweis immer, wenn der Spieler in der Naehe ist
        if (interactionHint != null)
            interactionHint.SetActive(playerInRange);

        if (playerInRange && Input.GetMouseButtonDown(0) && IsMouseOver() && HasRequiredItem())
            RemoveWire();
    }

    private bool IsMouseOver()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;

        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        foreach (Collider2D col in clickColliders)
            if (col != null && col.enabled && col.OverlapPoint(mouse))
                return true;

        // Kein Collider getroffen -> Sprite-Flaeche als Klickbereich
        foreach (Renderer r in renderers)
            if (r != null && r.bounds.Contains(new Vector3(mouse.x, mouse.y, r.bounds.center.z)))
                return true;

        return false;
    }

    private bool HasRequiredItem()
    {
        if (mainHandSlot == null) return false;

        var field = typeof(EquippedSlot).GetField(
            "itemName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field != null && (field.GetValue(mainHandSlot) as string) == requiredItemName;
    }

    private void RemoveWire()
    {
        Debug.Log("Wire entfernt mit: " + requiredItemName);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}