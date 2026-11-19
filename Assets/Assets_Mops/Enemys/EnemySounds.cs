using UnityEngine;

/// <summary>
/// Spielt Gegner-Sounds ab: 2D, aber leiser je weiter der Gegner von der Kamera weg ist.
/// Leichte Tonhoehen-Variation, damit sich viele gleiche Sounds nicht monoton anhoeren.
/// </summary>
public static class EnemySound
{
    public static float fullVolumeDistance = 8f;   // bis hier volle Lautstaerke
    public static float silentDistance = 20f;      // ab hier nichts mehr

    public static void Play(Component owner, AudioClip clip, float volume = 1f, float pitchVariation = 0.08f)
    {
        if (clip == null || owner == null) return;

        float dist = Camera.main != null
            ? Vector2.Distance(owner.transform.position, Camera.main.transform.position)
            : 0f;
        float fade = 1f - Mathf.InverseLerp(fullVolumeDistance, silentDistance, dist);
        if (fade <= 0f) return;

        AudioSource src = GetSource(owner);
        src.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        src.PlayOneShot(clip, volume * fade);
    }

    public static AudioSource GetSource(Component owner)
    {
        AudioSource src = owner.GetComponent<AudioSource>();
        if (src == null)
        {
            src = owner.gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
        }
        return src;
    }
}