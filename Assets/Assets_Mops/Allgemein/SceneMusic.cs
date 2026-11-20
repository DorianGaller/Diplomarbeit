using UnityEngine;

/// <summary>
/// Welche Musik in dieser Szene laeuft. Pro Szene ein leeres GameObject "SceneMusic" mit diesem Script.
/// Ohne SceneMusic in einer Szene blendet die Musik aus (z.B. BaseScene).
/// </summary>
public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [SerializeField] private float fadeTime = 1.5f;
    [Tooltip("Sekunden Stille nach dem Ende des Stuecks, dann beginnt es von vorne (0 = nahtlos)")]
    [SerializeField] private float pauseAfterTrack = 4f;

    private void Start() => PlayNow(fadeTime);

    public void PlayNow(float fade) => MusicManager.Get().Play(music, fade, volume, pauseAfterTrack);
}