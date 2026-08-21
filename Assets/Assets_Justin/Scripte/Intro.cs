using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Intro : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string nextSceneName = "Press_Any_Button Screen";

    [Header("Text")]
    [SerializeField] private TMP_Text introText;
    [SerializeField] private TMP_Text easterEggText;
    private static readonly string[] EasterEggPhrases =
    {
        "Pack you Rock Star Games!",
        "Wo ist der Onkel?",
        "Mops war hier!",
        "Toast war hier!",
        "Galli war hier!",
        "Skili war hier!",
        "Ymom2 war hier!",
        "ESCyber bevor GTA6?!",
        "Irfan der Klassenbeste",
        "End Update",

    };

    [SerializeField] private Canvas imageCanvas;
    [SerializeField] private Image imageDisplay;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite finalSprite;

    [Header("Image Scale")]
    [SerializeField] private float selectedScale = 1f;
    [SerializeField] private float finalScale = 1f;
    [SerializeField] private float imageVerticalOffset = -80f;

    [Header("Helper Text")]
    [SerializeField] private TMP_Text chatGptText;
    [SerializeField] private TMP_Text claudeText;
    [SerializeField] private TMP_Text pixelLabAiText;
    [SerializeField] private float helperSpacing = 40f;

    [Header("Background")]
    [SerializeField] private Image backgroundDisplay;
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private float backgroundScale = 1f;

    [SerializeField] private float letterDelay = 0.06f;
    [SerializeField] private float dotDelay = 0.45f;
    [SerializeField] private float imageFadeDuration = 0.7f;
    [SerializeField] private float textFadeDuration = 0.7f;
    [SerializeField] private float easterEggFadeInDuration = 0.25f;
    [SerializeField] private float helperFadeDuration = 0.35f;
    [SerializeField] private float helperFadeDelay = 0.15f;

    [Header("Stage Text Y Positions")]
    [SerializeField] private float firstTextY = 100f;
    [SerializeField] private float helperTextY = 100f;
    [SerializeField] private float finalTextY = 100f;
    [SerializeField] private float easterEggTextY = -200f;
    [SerializeField] private float easterEggTiltAngle = 4f;
    [SerializeField] private float easterEggTiltSpeed = 0.8f;

    private bool sceneLoading;
    private float easterEggTiltDirection;

    private void Start()
    {
        FindCanvasImages();
        easterEggTiltDirection = Random.value < 0.5f ? -1f : 1f;
        PrepareDisplay();
        PrepareBackground();
        StartCoroutine(PlayIntro());
    }

    private void Update()
    {
        KeepBackgroundVisible();
        AnimateEasterEggGradient();

        if (Input.GetMouseButtonDown(0) && !sceneLoading)
        {
            LoadNextScene();
        }
    }

    private System.Collections.IEnumerator PlayIntro()
    {
        yield return WaitForDuration(1f);

        SetSpriteLayout(selectedSprite, selectedScale);
        SetTextY(introText, firstTextY);
        yield return TypeText("A Diploma Game Project from", letterDelay);
        yield return ShowSprite(selectedSprite, selectedScale, imageFadeDuration);
        yield return WaitForDuration(1.5f);
        yield return ShowEasterEgg();

        yield return ClearTextAndImages();
        yield return WaitForDuration(2f);

        if (introText != null)
        {
            introText.text = "Created with the assistance of";
            SetTextY(introText, helperTextY);
            SetTextAlpha(1f);
        }
        yield return WaitForDuration(1f);
        yield return ShowHelperTexts();

        yield return WaitForDuration(0.25f);
        yield return ClearTextAndImages();
        yield return WaitForDuration(2f);

        SetTextY(introText, finalTextY);
        yield return TypeText("The ESC-Dev-Team Presents . . .", letterDelay * 2f);
        if (introText != null)
        {
            introText.text = string.Empty;
        }
        if (easterEggText != null)
        {
            easterEggText.text = string.Empty;
        }
        yield return ShowSprite(finalSprite, finalScale, imageFadeDuration);
        yield return WaitForDuration(2f);
        yield return HideDisplay();

        LoadNextScene();
    }

    private System.Collections.IEnumerator WaitForDuration(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private System.Collections.IEnumerator TypeText(string text, float characterDelay)
    {
        if (introText == null)
        {
            yield break;
        }

        introText.text = string.Empty;
        introText.alignment = TextAlignmentOptions.Center;
        SetTextAlpha(0f);
        float elapsed = 0f;
        foreach (char character in text)
        {
            introText.text += character;
            elapsed += characterDelay;
            SetTextAlpha(Mathf.Clamp01(elapsed / textFadeDuration));
            yield return WaitForDuration(character == '.' ? dotDelay : characterDelay);
        }

        yield return FadeText(1f);
    }

    private System.Collections.IEnumerator ShowSprite(Sprite sprite, float scale, float fadeDuration)
    {
        if (imageDisplay == null || sprite == null)
        {
            yield break;
        }

        SetSpriteLayout(sprite, scale);
        imageDisplay.gameObject.SetActive(true);
        yield return FadeDisplay(1f, fadeDuration);
    }

    private void SetSpriteLayout(Sprite sprite, float scale)
    {
        if (imageDisplay == null || sprite == null)
        {
            return;
        }

        imageDisplay.sprite = sprite;
        imageDisplay.SetNativeSize();
        float safeScale = Mathf.Max(0.01f, scale);
        imageDisplay.transform.localScale = Vector3.one * safeScale;
        imageDisplay.rectTransform.anchoredPosition = new Vector2(0f, imageVerticalOffset);
    }

    private System.Collections.IEnumerator HideDisplay()
    {
        if (imageDisplay != null)
        {
            yield return FadeImage(imageDisplay, 0f, imageFadeDuration);
            imageDisplay.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator FadeDisplay(float targetAlpha, float fadeDuration)
    {
        yield return FadeImage(imageDisplay, targetAlpha, fadeDuration);
    }

    private System.Collections.IEnumerator FadeImage(Image image, float targetAlpha, float fadeDuration)
    {
        if (image == null)
        {
            yield break;
        }

        Color startColor = image.color;
        float startAlpha = startColor.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            Color color = image.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            image.color = color;
            yield return null;
        }

        Color finalColor = image.color;
        finalColor.a = targetAlpha;
        image.color = finalColor;
    }

    private void PrepareDisplay()
    {
        if (imageDisplay == null)
        {
            return;
        }

        RectTransform displayTransform = imageDisplay.rectTransform;
        displayTransform.anchorMin = new Vector2(0.5f, 0.5f);
        displayTransform.anchorMax = new Vector2(0.5f, 0.5f);
        displayTransform.pivot = new Vector2(0.5f, 0.5f);
        displayTransform.anchoredPosition = Vector2.zero;
        displayTransform.localRotation = Quaternion.identity;
        imageDisplay.preserveAspect = true;
        SetImageAlpha(imageDisplay, 0f);

        if (easterEggText != null)
        {
            easterEggText.gameObject.SetActive(false);
            SetTextAlpha(easterEggText, 0f);
            easterEggText.enableVertexGradient = true;
            easterEggText.rectTransform.localRotation = Quaternion.identity;
        }

        foreach (TMP_Text helperText in GetHelperTexts())
        {
            SetTextAlpha(helperText, 0f);
            if (helperText != null)
            {
                helperText.gameObject.SetActive(false);
            }
        }
    }

    private System.Collections.IEnumerator ShowEasterEgg()
    {
        if (easterEggText == null || EasterEggPhrases.Length == 0)
        {
            yield break;
        }

        SetTextY(easterEggText, easterEggTextY);
        easterEggText.text = EasterEggPhrases[Random.Range(0, EasterEggPhrases.Length)];
        easterEggText.alignment = TextAlignmentOptions.Center;
        easterEggText.gameObject.SetActive(true);
        easterEggText.rectTransform.localRotation = Quaternion.identity;
        SetTextAlpha(easterEggText, 0f);
        yield return FadeTextElement(easterEggText, 1f, easterEggFadeInDuration);
    }

    private static void SetTextY(TMP_Text text, float y)
    {
        if (text == null)
        {
            return;
        }

        RectTransform textTransform = text.rectTransform;
        textTransform.anchorMin = new Vector2(0.5f, 0.5f);
        textTransform.anchorMax = new Vector2(0.5f, 0.5f);
        textTransform.pivot = new Vector2(0.5f, 0.5f);
        textTransform.anchoredPosition = new Vector2(0f, y);
    }

    private void AnimateEasterEggGradient()
    {
        if (easterEggText == null || !easterEggText.gameObject.activeInHierarchy)
        {
            return;
        }

        float wave = (Mathf.Sin(Time.time * 2.5f) + 1f) * 0.5f;
        Color brightGold = Color.Lerp(new Color(1f, 0.55f, 0.02f), new Color(1f, 0.95f, 0.25f), wave);
        Color darkGold = Color.Lerp(new Color(0.65f, 0.2f, 0.01f), new Color(1f, 0.65f, 0.03f), wave);
        easterEggText.colorGradient = new VertexGradient(brightGold, brightGold, darkGold, darkGold);

        float tilt = Mathf.Sin(Time.time * easterEggTiltSpeed) * easterEggTiltAngle * easterEggTiltDirection;
        easterEggText.rectTransform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    private System.Collections.IEnumerator ShowHelperTexts()
    {
        TMP_Text[] helperTexts = GetHelperTexts();
        string[] labels = { "ChatGPT", "Claude", "PixelLab AI" };
        float totalWidth = 0f;
        float[] widths = new float[helperTexts.Length];

        for (int index = 0; index < helperTexts.Length; index++)
        {
            if (helperTexts[index] == null)
            {
                continue;
            }

            helperTexts[index].text = labels[index];
            helperTexts[index].alignment = TextAlignmentOptions.Center;
            helperTexts[index].gameObject.SetActive(true);
            SetTextAlpha(helperTexts[index], 0f);
            widths[index] = helperTexts[index].preferredWidth;
            totalWidth += widths[index];
        }

        totalWidth += helperSpacing * Mathf.Max(0, CountValidHelpers(helperTexts) - 1);
        float currentX = -totalWidth * 0.5f;

        for (int index = 0; index < helperTexts.Length; index++)
        {
            if (helperTexts[index] == null)
            {
                continue;
            }

            RectTransform helperTransform = helperTexts[index].rectTransform;
            helperTransform.anchorMin = new Vector2(0.5f, 0.5f);
            helperTransform.anchorMax = new Vector2(0.5f, 0.5f);
            helperTransform.pivot = new Vector2(0.5f, 0.5f);
            helperTransform.anchoredPosition = new Vector2(currentX + widths[index] * 0.5f, 0f);
            yield return FadeTextElement(helperTexts[index], 1f, helperFadeDuration);
            yield return WaitForDuration(helperFadeDelay);
            currentX += widths[index] + helperSpacing;
        }
    }

    private static int CountValidHelpers(TMP_Text[] helperTexts)
    {
        int count = 0;
        for (int index = 0; index < helperTexts.Length; index++)
        {
            if (helperTexts[index] != null)
            {
                count++;
            }
        }

        return count;
    }

    private void FindCanvasImages()
    {
        if (imageCanvas == null)
        {
            return;
        }

        Image[] canvasImages = imageCanvas.GetComponentsInChildren<Image>(true);
        if (backgroundDisplay == null)
        {
            foreach (Image canvasImage in canvasImages)
            {
                if (canvasImage.gameObject.name.ToLowerInvariant().Contains("background"))
                {
                    backgroundDisplay = canvasImage;
                    break;
                }
            }
        }

        if (imageDisplay == null)
        {
            foreach (Image canvasImage in canvasImages)
            {
                if (canvasImage != backgroundDisplay)
                {
                    imageDisplay = canvasImage;
                    break;
                }
            }
        }

        if (backgroundDisplay == null)
        {
            foreach (Image canvasImage in canvasImages)
            {
                if (canvasImage != imageDisplay)
                {
                    backgroundDisplay = canvasImage;
                    break;
                }
            }
        }

        if (imageDisplay == backgroundDisplay)
        {
            imageDisplay = null;
            foreach (Image canvasImage in canvasImages)
            {
                if (canvasImage != backgroundDisplay)
                {
                    imageDisplay = canvasImage;
                    break;
                }
            }
        }

    }

    private System.Collections.IEnumerator ClearTextAndImages()
    {
        Coroutine textFade = null;
        Coroutine easterEggFade = null;
        Coroutine imageFade = null;
        Coroutine[] helperTextFades = new Coroutine[3];

        if (introText != null)
        {
            textFade = StartCoroutine(FadeText(0f));
        }

        if (easterEggText != null)
        {
            easterEggFade = StartCoroutine(FadeTextElement(easterEggText, 0f));
        }

        TMP_Text[] helperTexts = GetHelperTexts();
        for (int index = 0; index < helperTexts.Length; index++)
        {
            if (helperTexts[index] != null)
            {
                helperTextFades[index] = StartCoroutine(FadeTextElement(helperTexts[index], 0f));
            }
        }

        if (imageDisplay != null)
        {
            imageFade = StartCoroutine(FadeImage(imageDisplay, 0f, imageFadeDuration));
        }

        if (textFade != null)
        {
            yield return textFade;
        }

        if (easterEggFade != null)
        {
            yield return easterEggFade;
            easterEggText.gameObject.SetActive(false);
        }

        if (imageFade != null)
        {
            yield return imageFade;
            SetImageInactive(imageDisplay);
        }

        for (int index = 0; index < helperTextFades.Length; index++)
        {
            if (helperTextFades[index] != null)
            {
                yield return helperTextFades[index];
                helperTexts[index].gameObject.SetActive(false);
            }
        }

        if (introText != null)
        {
            introText.text = string.Empty;
        }
    }

    private System.Collections.IEnumerator FadeText(float targetAlpha)
    {
        if (introText == null)
        {
            yield break;
        }

        Color startColor = introText.color;
        float startAlpha = startColor.a;
        float elapsed = 0f;

        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / textFadeDuration));
            yield return null;
        }

        SetTextAlpha(targetAlpha);
    }

    private System.Collections.IEnumerator FadeTextElement(TMP_Text text, float targetAlpha)
    {
        if (text == null)
        {
            yield break;
        }

        float startAlpha = text.color.a;
        float elapsed = 0f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(text, Mathf.Lerp(startAlpha, targetAlpha, elapsed / textFadeDuration));
            yield return null;
        }

        SetTextAlpha(text, targetAlpha);
    }

    private System.Collections.IEnumerator FadeTextElement(TMP_Text text, float targetAlpha, float duration)
    {
        if (text == null)
        {
            yield break;
        }

        float startAlpha = text.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(text, Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetTextAlpha(text, targetAlpha);
    }

    private void SetTextAlpha(float alpha)
    {
        if (introText == null)
        {
            return;
        }

        Color color = introText.color;
        color.a = alpha;
        introText.color = color;
    }

    private static void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null)
        {
            return;
        }

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private void PrepareBackground()
    {
        if (backgroundDisplay == null || backgroundSprite == null)
        {
            return;
        }

        backgroundDisplay.sprite = backgroundSprite;
        RectTransform backgroundTransform = backgroundDisplay.rectTransform;
        backgroundTransform.anchorMin = Vector2.zero;
        backgroundTransform.anchorMax = Vector2.one;
        backgroundTransform.offsetMin = Vector2.zero;
        backgroundTransform.offsetMax = Vector2.zero;
        backgroundTransform.localScale = Vector3.one * Mathf.Max(0.01f, backgroundScale);
        backgroundDisplay.preserveAspect = false;
        SetImageAlpha(backgroundDisplay, 1f);
        backgroundDisplay.gameObject.SetActive(true);
        backgroundDisplay.transform.SetAsFirstSibling();
    }

    private void KeepBackgroundVisible()
    {
        if (backgroundDisplay == null || backgroundDisplay == imageDisplay)
        {
            return;
        }

        Color color = backgroundDisplay.color;
        color.a = 1f;
        backgroundDisplay.color = color;
        backgroundDisplay.gameObject.SetActive(true);
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
        image.gameObject.SetActive(alpha > 0f);
    }

    private static void SetImageInactive(Image image)
    {
        if (image != null)
        {
            image.gameObject.SetActive(false);
        }
    }

    private TMP_Text[] GetHelperTexts()
    {
        return new[] { chatGptText, claudeText, pixelLabAiText };
    }

    private void LoadNextScene()
    {
        sceneLoading = true;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("No next scene name assigned for the intro.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
