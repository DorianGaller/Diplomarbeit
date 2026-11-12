using UnityEngine;

/// <summary>
/// Kommt auf das Desktop-Objekt vom PC. Zeigt das Audio-Log-Icon erst,
/// wenn der passende USB-Chip aufgesammelt wurde.
/// </summary>
public class AudioLogDesktopIcon : MonoBehaviour
{
    [SerializeField] private string logId = "lab_log_01";
    [SerializeField] private GameObject icon;

    void OnEnable()
    {
        if (icon != null)
            icon.SetActive(SaveSystem.HasLog(logId));
    }
}