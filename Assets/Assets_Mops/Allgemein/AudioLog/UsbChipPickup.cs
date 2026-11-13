using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// USB-Chip, der beim Beruehren aufgesammelt wird.
/// Merkt sich ueber den Spielstand, dass der Log gefunden wurde -> danach am PC in der Base abspielbar.
/// Braucht einen Collider2D mit "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class UsbChipPickup : MonoBehaviour
{
    [Tooltip("Muss mit der Log Id an der AudioLogApp und am AudioLogDesktopIcon uebereinstimmen")]
    [SerializeField] private string logId = "lab_log_01";

    [Header("Meldung beim Aufsammeln (optional)")]
    [Tooltip("World-Space TextMeshPro als Kind-Objekt")]
    [SerializeField] private TMP_Text pickupLabel;
    [SerializeField] private string pickupMessage = "USB chip found\nPlug it into the PC at the base";
    [SerializeField] private float messageDuration = 2.5f;

    [Header("Schweben")]
    [SerializeField] private float bobHeight = 0.06f;
    [SerializeField] private float bobSpeed = 2.5f;

    private Vector3 startPos;
    private bool collected;

    void Start()
    {
        // Schon gefunden -> nicht nochmal hinlegen
        if (SaveSystem.HasLog(logId))
        {
            Destroy(gameObject);
            return;
        }

        startPos = transform.position;
        if (pickupLabel != null) pickupLabel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (collected) return;
        transform.position = startPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        collected = true;
        SaveSystem.CollectLog(logId);
        StartCoroutine(PickupRoutine());
    }

    IEnumerator PickupRoutine()
    {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;
        foreach (Collider2D col in GetComponents<Collider2D>()) col.enabled = false;

        if (pickupLabel != null)
        {
            pickupLabel.text = pickupMessage;
            pickupLabel.gameObject.SetActive(true);

            Color baseColor = pickupLabel.color;
            Vector3 labelStart = pickupLabel.transform.position;

            for (float t = 0f; t < messageDuration; t += Time.deltaTime)
            {
                float k = t / messageDuration;
                pickupLabel.transform.position = labelStart + Vector3.up * (k * 0.4f);

                Color c = baseColor;
                c.a = baseColor.a * (k < 0.7f ? 1f : 1f - (k - 0.7f) / 0.3f);   // am Ende ausblenden
                pickupLabel.color = c;
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}