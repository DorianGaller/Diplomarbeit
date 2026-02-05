using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartUI : MonoBehaviour
{
    public Image[] hearts;        // alle Herz-Images (links → rechts)
    public float fadeDuration = 0.3f;

    private int lastHeartCount;

    public void UpdateHearts(int currentHP, int maxHP)
    {
        int maxHearts = hearts.Length;
        int heartsToShow = Mathf.CeilToInt((float)currentHP / maxHP * maxHearts);

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < heartsToShow)
            {
                hearts[i].gameObject.SetActive(true);
                SetAlpha(hearts[i], 1f);
            }
            else
            {
                if (hearts[i].gameObject.activeSelf)
                    StartCoroutine(FadeOut(hearts[i]));

                hearts[i].gameObject.SetActive(false);
            }
        }

        lastHeartCount = heartsToShow;
    }

    IEnumerator FadeOut(Image img)
    {
        float t = 0;
        Color c = img.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            img.color = c;
            yield return null;
        }
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
