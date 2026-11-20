using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    public GameObject bossBarRoot;
    public Image healthFill;
    public TMP_Text phaseText;

    [Header("Animation")]
    public float fillSpeed = 3f;

    private Coroutine fillCoroutine;

    public void ShowBossBar(bool show)
    {
        if (bossBarRoot != null)
            bossBarRoot.SetActive(show);
    }

    public void SetHealth(int current, int max, int phase)
    {
        float target = max > 0 ? (float)current / max : 0f;

        if (phaseText != null)
            phaseText.text = phase.ToString();

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(AnimateFill(target));
    }

    IEnumerator AnimateFill(float target)
    {
        while (healthFill != null && Mathf.Abs(healthFill.fillAmount - target) > 0.001f)
        {
            healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, target, fillSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        if (healthFill != null)
            healthFill.fillAmount = target;
    }
}