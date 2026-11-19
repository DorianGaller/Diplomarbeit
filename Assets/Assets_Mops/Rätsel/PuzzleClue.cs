using TMPro;
using UnityEngine;

/// <summary>
/// Hinweis-Zettel/Aufkleber im Raum. Holt sich den Text von einer Station,
/// z.B. "PORT 3 -> CORE" vom Patch-Panel oder das Handbuch vom Kabelschacht.
/// Kommt auf ein Objekt mit TextMeshPro (3D-Text, nicht UI).
/// </summary>
public class PuzzleClue : MonoBehaviour
{
    [SerializeField] private PuzzleStation station;
    [Tooltip("Patch-Panel: 0 = Port 1, 1 = Port 2 ... Beim Kabelschacht egal")]
    [SerializeField] private int index;

    private void Start()
    {
        TMP_Text text = GetComponent<TMP_Text>();
        if (text == null || station == null)
        {
            Debug.LogWarning("[PuzzleClue] TextMeshPro oder Station fehlt an " + name);
            return;
        }

        text.text = station.GetClue(index);
    }
}