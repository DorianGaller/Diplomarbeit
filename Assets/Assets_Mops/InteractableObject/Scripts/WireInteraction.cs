using UnityEngine;
using System.Collections;

public class WireInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string requiredItemName = "Stahlschere";
    [SerializeField] private float interactionRange = 2f;

    [Header("References")]
    [SerializeField] private EquippedSlot mainHandSlot;
    [SerializeField] private GameObject interactionHint;

    private Transform player;
    private bool playerInRange = false;

    private void Start()
    {
        StartCoroutine(InitNextFrame());   // NEU
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
            // NEU: FindObjectsInactive.Include, damit auch der (noch geschlossene) Equipment-Slot gefunden wird
            EquippedSlot[] allSlots = FindObjectsByType<EquippedSlot>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            var itemTypeField = typeof(EquippedSlot).GetField(
                "itemType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            foreach (EquippedSlot slot in allSlots)
            {
                if (itemTypeField != null)
                {
                    object value = itemTypeField.GetValue(slot);
                    if (value is ItemType type && type == ItemType.mainHand)
                    {
                        mainHandSlot = slot;
                        break;
                    }
                }
            }

            if (mainHandSlot == null)
                Debug.LogError("WireInteraction: MainHand EquippedSlot nicht gefunden! " +
                               "Bitte im Inspector manuell zuweisen.");
        }

        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    private void Update()
    {
        if (player == null || mainHandSlot == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        bool hasSchere = HasRequiredItem();

        if (interactionHint != null)
            interactionHint.SetActive(playerInRange && hasSchere);

        if (playerInRange && hasSchere && Input.GetKeyDown(KeyCode.F))
        {
            RemoveWire();
        }
    }

    private bool HasRequiredItem()
    {
        var field = typeof(EquippedSlot).GetField(
            "itemName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field == null)
        {
            Debug.LogError("WireInteraction: Feld 'itemName' in EquippedSlot nicht gefunden!");
            return false;
        }

        string equippedName = field.GetValue(mainHandSlot) as string;
        return equippedName == requiredItemName;
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