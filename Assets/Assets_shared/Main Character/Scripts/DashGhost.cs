using UnityEngine;
using System.Collections;

public class DashGhost : MonoBehaviour
{
    public float lifeTime = 0.15f;
    public float fadeSpeed = 5f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        Color color = spriteRenderer.color;

        while (color.a > 0)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}
