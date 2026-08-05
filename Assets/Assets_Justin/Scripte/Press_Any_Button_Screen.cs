using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Press_Any_Button_Screen : MonoBehaviour
{
    [Header("Camera Descent")]
    [Tooltip("The camera that should move. If unset, this GameObject's transform is used.")]
    public Camera targetCamera;

    [Tooltip("Use an absolute final Y target. If enabled, targetY is used instead of moveDistance.")]
    private bool useAbsoluteY = true;

    [Tooltip("The absolute Y position the camera should move to when useAbsoluteY is enabled.")]
    private float targetY = -15f;

    [Tooltip("How far down the camera should move from its starting position when useAbsoluteY is disabled.")]
    private float moveDistance = 5f;

    [Tooltip("How fast the camera should move. Higher values move faster.")]
    private float moveSpeed = 20f;

    [Tooltip("How far back the camera should move during the acceleration phase.")]
    private float backDistance = 8f;

    [Tooltip("Curve that controls the back-and-forth offset while the camera accelerates and then returns.")]
    private AnimationCurve backCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );

    [Tooltip("How much the camera should initially zoom out before moving.")]
    private float zoomOutAmount = 4f;

    [Tooltip("How much closer the camera should be at the end compared to its starting size.")]
    private float zoomInAmount = 1.5f;

    [Tooltip("How much the camera zoom oscillates while descending.")]
    private float initialZoomOscillationAmount = 0.5f;

    [Tooltip("How fast the camera zoom oscillates while descending.")]
    private float initialZoomOscillationFrequency = 3.5f;

    [Tooltip("Use a custom curve for the motion. The default curve starts accelerating faster and earlier.")]
    private AnimationCurve moveCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 2f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [Tooltip("How much faster the camera moves while the player is holding a button.")]
    private float holdSpeedMultiplier = 4f;

    [Tooltip("Force the target Z position after the move, which is useful for 2D cameras.")]
    private bool clampTargetZ = true;

    [Tooltip("The Z position to clamp to when clampTargetZ is enabled.")]
    private float targetZ = -10f;

    [Header("Canvas Fade")]
    [Tooltip("The UI Image panel that will be faded.")]
    public Image fadePanel;

    [Tooltip("The TextMeshProUGUI text that updates after the camera arrives and on input.")]
    public TextMeshProUGUI pressAnyButtonText;

    [Tooltip("How long the press-any-button text takes to fade in.")]
    private float pressAnyButtonFadeInDuration = 0.8f;

    [Tooltip("How long the press-any-button text takes to fade out.")]
    private float pressAnyButtonFadeOutDuration = 0.15f;

    [Tooltip("Maximum tilt angle for the press-any-button text.")]
    private float pressAnyButtonTiltAngle = 4f;

    [Tooltip("How fast the press-any-button text tilts.")]
    private float pressAnyButtonTiltFrequency = 1.2f;

    private float pressAnyButtonAlpha;
    private float pressAnyButtonTargetAlpha;
    private RectTransform pressAnyButtonRect;

    [Tooltip("Color used when the panel is fully visible.")]
    private Color fadeStartColor = Color.black;

    [Tooltip("Color used when the panel is faded out.")]
    private Color fadeEndColor = Color.black;

    [Tooltip("Starting alpha value (1 = fully visible).")]
    [Range(0f, 1f)]
    private float fadeStartAlpha = 1f;

    [Tooltip("Ending alpha value. 100/255 is the default target alpha.")]
    [Range(0f, 1f)]
    private float fadeEndAlpha = 100f / 255f;

    [Tooltip("How long the panel fade takes in seconds.")]
    private float fadeDuration = 1f;

    [Tooltip("Automatically start the fade when enabled.")]
    private bool startFadeOnEnable = true;

    [Tooltip("How long to wait after the camera has arrived before the player can trigger the final zoom/fade.")]
    private float postArrivalWaitTime = 1f;

    [Tooltip("How much closer the camera should zoom in after the final arrival and player input.")]
    private float postArrivalZoomAmount = 12f;

    [Tooltip("How much the camera should quickly zoom out after input before snapping in.")]
    private float postArrivalZoomOutAmount = 1.5f;

    [Tooltip("How long the initial post-click zoom-out takes.")]
    private float postArrivalZoomOutDuration = 0.25f;

    [Tooltip("How long the post-arrival zoom-in takes.")]
    private float postArrivalZoomDuration = 0.18f;

    [Tooltip("How long the final white fade takes before the scene changes.")]
    private float postArrivalFadeDuration = 0.1f;

    [Tooltip("Name of the scene to load after the final zoom completes.")]
    private string nextSceneName = "Main_Menue";

    [Header("Camera Shake")]
    [Tooltip("How fast the shake oscillates.")]
    private float shakeFrequency = 6f;

    [Tooltip("Maximum tilt angle in degrees when the camera sways.")]
    private float shakeTiltAngle = 1.5f;

    private Transform cameraTransform;
    private Camera activeCamera;
    private Vector3 startPosition;
    private Vector3 finalPosition;
    private float startOrthographicSize;
    private float finalOrthographicSize;
    private float progress;
    private bool initialized;
    private Quaternion initialCameraRotation;

    private enum CameraState
    {
        Moving,
        WaitingForInput,
        ZoomingIn,
        Completed
    }

    private CameraState currentState = CameraState.Moving;
    private float arrivalCompleteTime;
    private Coroutine arrivalZoomRoutine;

    private void Reset()
    {
        moveSpeed = 20f;
        backDistance = 8f;
        zoomOutAmount = 4f;
        zoomInAmount = 1.5f;
        holdSpeedMultiplier = 4f;
        fadeDuration = 1f;
        shakeFrequency = 5f;
        shakeTiltAngle = 2.5f;
        fadeStartAlpha = 1f;
        fadeEndAlpha = 100f / 255f;
        startFadeOnEnable = true;
        pressAnyButtonTiltFrequency = 1.2f;
        postArrivalWaitTime = 1f;
        postArrivalZoomAmount = 12f;
        postArrivalZoomOutAmount = 1.5f;
        postArrivalZoomOutDuration = 0.25f;
        postArrivalZoomDuration = 0.18f;
        postArrivalFadeDuration = 0.1f;
        nextSceneName = "Main_Menue";
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        cameraTransform = targetCamera != null ? targetCamera.transform : transform;
        if (cameraTransform == null)
        {
            cameraTransform = transform;
        }

        startPosition = cameraTransform.position;
        finalPosition = useAbsoluteY ? new Vector3(startPosition.x, targetY, startPosition.z) : startPosition + Vector3.down * moveDistance;

        if (clampTargetZ)
        {
            finalPosition.z = targetZ;
        }

        activeCamera = targetCamera != null ? targetCamera : cameraTransform.GetComponent<Camera>();

        if (activeCamera != null)
        {
            startOrthographicSize = activeCamera.orthographicSize;
            finalOrthographicSize = startOrthographicSize - zoomInAmount;
        }

        initialCameraRotation = cameraTransform.rotation;
        progress = 0f;
        currentState = CameraState.Moving;
        arrivalZoomRoutine = null;
        initialized = true;

        pressAnyButtonRect = pressAnyButtonText != null ? pressAnyButtonText.rectTransform : null;
        pressAnyButtonAlpha = 0f;
        pressAnyButtonTargetAlpha = 0f;

        if (pressAnyButtonText != null)
        {
            pressAnyButtonText.text = string.Empty;
            Color textColor = pressAnyButtonText.color;
            textColor.a = 0f;
            pressAnyButtonText.color = textColor;
        }

        UpdateCamera();

        if (startFadeOnEnable)
        {
            StartCoroutine(FadePanelRoutine());
        }
    }

    public void StartPanelFade()
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Press_Any_Button_Screen: fadePanel is not assigned.");
            return;
        }

        StopCoroutine(FadePanelRoutine());
        StartCoroutine(FadePanelRoutine());
    }

    private IEnumerator FadePanelRoutine()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Color fromColor = fadeStartColor;
        fromColor.a = fadeStartAlpha;
        Color toColor = fadeEndColor;
        toColor.a = fadeEndAlpha;

        fadePanel.color = fromColor;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = Color.Lerp(fromColor, toColor, t);
            yield return null;
        }

        fadePanel.color = toColor;
    }

    private void Update()
    {
        if (!initialized)
        {
            Initialize();
        }

        if (currentState == CameraState.Moving)
        {
            bool isHolding = Input.anyKey;
            float distance = Mathf.Abs(useAbsoluteY ? startPosition.y - targetY : moveDistance);
            float speed = moveSpeed * (isHolding ? holdSpeedMultiplier : 1f);
            float deltaProgress = distance <= 0.001f ? 1f : speed * Time.deltaTime / distance;

            progress = Mathf.MoveTowards(progress, 1f, deltaProgress);

            if (progress >= 1f)
            {
                currentState = CameraState.WaitingForInput;
                arrivalCompleteTime = Time.time;

                if (pressAnyButtonText != null)
                {
                    pressAnyButtonText.text = "Press any Button to Start";
                    pressAnyButtonTargetAlpha = 1f;
                }
            }

            UpdateCamera();
            return;
        }

        if (currentState == CameraState.WaitingForInput)
        {
            UpdateCamera();

            if (Time.time - arrivalCompleteTime >= postArrivalWaitTime && Input.anyKeyDown)
            {
                StartArrivalZoomSequence();
            }

            return;
        }

        if (currentState == CameraState.ZoomingIn)
        {
            UpdateCamera();
        }
    }

    private void UpdateCamera()
    {
        float easedProgress = EvaluateCurve(progress);
        Vector3 offset = Vector3.down * backDistance * backCurve.Evaluate(progress);
        Vector3 basePosition = Vector3.Lerp(startPosition, finalPosition, easedProgress) + offset;

        if (currentState == CameraState.Moving)
        {
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            float tiltFactor = 1f - smoothProgress;
            float tiltAmount = Mathf.Sin(Time.time * shakeFrequency) * shakeTiltAngle * tiltFactor;
            cameraTransform.rotation = initialCameraRotation * Quaternion.Euler(0f, 0f, tiltAmount);
        }
        else
        {
            cameraTransform.rotation = initialCameraRotation;
        }

        cameraTransform.position = basePosition;

        UpdatePressAnyButtonText(Time.deltaTime);

        if (activeCamera != null && currentState != CameraState.ZoomingIn)
        {
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            float baseZoom = Mathf.Lerp(startOrthographicSize + zoomOutAmount, finalOrthographicSize, smoothProgress);
            float oscillationFalloff = 1f - smoothProgress;
            float oscillation = Mathf.Sin(Time.time * initialZoomOscillationFrequency) * initialZoomOscillationAmount * oscillationFalloff;
            float targetZoom = baseZoom + oscillation;
            float minZoom = Mathf.Min(startOrthographicSize, finalOrthographicSize) - initialZoomOscillationAmount;
            float maxZoom = Mathf.Max(startOrthographicSize + zoomOutAmount, finalOrthographicSize) + initialZoomOscillationAmount;
            activeCamera.orthographicSize = progress >= 1f ? finalOrthographicSize : Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }

    private float EvaluateCurve(float progress)
    {
        if (moveCurve != null && moveCurve.length > 0)
        {
            return moveCurve.Evaluate(progress);
        }

        return EaseInOutQuint(progress);
    }

    private static float EaseInOutQuint(float t)
    {
        return t < 0.5f
            ? 16f * t * t * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f;
    }

    private void UpdatePressAnyButtonText(float deltaTime)
    {
        if (pressAnyButtonText == null)
        {
            return;
        }

        if (!Mathf.Approximately(pressAnyButtonAlpha, pressAnyButtonTargetAlpha))
        {
            float duration = pressAnyButtonTargetAlpha > pressAnyButtonAlpha ? pressAnyButtonFadeInDuration : pressAnyButtonFadeOutDuration;
            pressAnyButtonAlpha = Mathf.MoveTowards(pressAnyButtonAlpha, pressAnyButtonTargetAlpha, deltaTime / Mathf.Max(0.001f, duration));
            Color currentColor = pressAnyButtonText.color;
            currentColor.a = pressAnyButtonAlpha;
            pressAnyButtonText.color = currentColor;
        }

        if (pressAnyButtonTargetAlpha > 0f)
        {
            if (pressAnyButtonRect != null)
            {
                float tilt = Mathf.Sin(Time.time * pressAnyButtonTiltFrequency) * pressAnyButtonTiltAngle;
                pressAnyButtonRect.localRotation = Quaternion.Euler(0f, 0f, tilt);
            }
        }
        else
        {
            if (pressAnyButtonRect != null)
            {
                pressAnyButtonRect.localRotation = Quaternion.identity;
            }
        }

        if (Mathf.Approximately(pressAnyButtonAlpha, 0f) && pressAnyButtonTargetAlpha <= 0f)
        {
            pressAnyButtonText.text = string.Empty;
        }
    }

    private void StartArrivalZoomSequence()
    {
        if (currentState != CameraState.WaitingForInput)
        {
            return;
        }

        if (pressAnyButtonText != null)
        {
            pressAnyButtonTargetAlpha = 0f;
        }

        if (arrivalZoomRoutine != null)
        {
            StopCoroutine(arrivalZoomRoutine);
        }

        currentState = CameraState.ZoomingIn;
        arrivalZoomRoutine = StartCoroutine(ArrivalZoomAndFadeRoutine());
    }

    private IEnumerator ArrivalZoomAndFadeRoutine()
    {
        if (fadePanel == null && activeCamera == null)
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        float startSize = activeCamera != null ? activeCamera.orthographicSize : 0f;
        float zoomOutTargetSize = activeCamera != null ? startSize + postArrivalZoomOutAmount : 0f;
        float targetSize = activeCamera != null ? Mathf.Max(0.1f, startSize - postArrivalZoomAmount) : 0f;

        float elapsed = 0f;
        while (elapsed < postArrivalZoomOutDuration)
        {
            elapsed += Time.deltaTime;

            if (activeCamera != null)
            {
                float zoomT = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, postArrivalZoomOutDuration));
                activeCamera.orthographicSize = Mathf.Lerp(startSize, zoomOutTargetSize, zoomT);
            }

            yield return null;
        }

        if (activeCamera != null)
        {
            activeCamera.orthographicSize = zoomOutTargetSize;
        }

        elapsed = 0f;
        while (elapsed < postArrivalZoomDuration)
        {
            elapsed += Time.deltaTime;

            if (activeCamera != null)
            {
                float zoomT = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, postArrivalZoomDuration));
                activeCamera.orthographicSize = Mathf.Lerp(zoomOutTargetSize, targetSize, zoomT);
            }

            yield return null;
        }

        if (activeCamera != null)
        {
            activeCamera.orthographicSize = targetSize;
        }

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(1f, 1f, 1f, 0f);

            float fadeElapsed = 0f;
            while (fadeElapsed < postArrivalFadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float fadeT = Mathf.Clamp01(fadeElapsed / Mathf.Max(0.001f, postArrivalFadeDuration));
                Color currentColor = fadePanel.color;
                currentColor.a = fadeT;
                fadePanel.color = currentColor;
                yield return null;
            }

            Color finalColor = fadePanel.color;
            finalColor.a = 1f;
            fadePanel.color = finalColor;
        }

        currentState = CameraState.Completed;
        SceneManager.LoadScene(nextSceneName);
    }
}

