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
    private float targetY = -13.5f;

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
    private float zoomInAmount = -0.5f;

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

    [Tooltip("The door Transform currently shown in view (starts closed).")]
    public Transform doorclosedObject;

    [Tooltip("The door Transform currently out of bounds (starts open, hidden).")]
    public Transform openDoorObject;

    [Tooltip("The object the camera should zoom in on (X/Y position) after the player clicks. If unset, the camera zooms in place.")]
    public Transform postClickZoomTarget;

    [Tooltip("Particle effects (e.g. rain) that should fade out during the final red transition so they don't render over the fade panel.")]
    public ParticleSystem[] particlesToFadeOnRedTransition;

    [Tooltip("How long the press-any-button text takes to fade in.")]
    private float pressAnyButtonFadeInDuration = 0.8f;

    [Tooltip("How long the press-any-button text takes to fade out.")]
    private float pressAnyButtonFadeOutDuration = 0.15f;

    [Tooltip("Maximum tilt angle for the press-any-button text.")]
    private float pressAnyButtonTiltAngle = 4f;

    [Tooltip("How fast the press-any-button text tilts.")]
    private float pressAnyButtonTiltFrequency = 1.2f;

    [Tooltip("First color of the animated gradient across the press-any-button text.")]
    private Color pressAnyButtonGradientColorA = new Color(0.15f, 1f, 0.1f);

    [Tooltip("Second color of the animated gradient across the press-any-button text.")]
    private Color pressAnyButtonGradientColorC = new Color(0.75f, 0.1f, 1f);

    [Tooltip("How fast the gradient scrolls across the press-any-button text.")]
    private float pressAnyButtonGradientSpeed = 0.5f;

    [Tooltip("How much the press-any-button text scales up/down, decoupled from the tilt for a more organic feel.")]
    private float pressAnyButtonScalePulseAmount = 0.08f;

    [Tooltip("How fast the scale pulse noise evolves.")]
    private float pressAnyButtonScaleNoiseSpeed = 0.7f;

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
    private float postArrivalZoomAmount = 40f;

    [Tooltip("How much the camera should quickly zoom out after input before snapping in.")]
    private float postArrivalZoomOutAmount = 0.5f;

    [Tooltip("How long the initial post-click zoom-out takes.")]
    private float postArrivalZoomOutDuration = 0.25f;

    [Tooltip("How far the camera tilts (in degrees) during the post-arrival flick.")]
    private float postArrivalTiltAngle = 4f;

    [Tooltip("How much further past postArrivalTiltAngle the camera overshoots before easing back to normal.")]
    private float postArrivalTiltOvershootExtra = 1.5f;

    [Tooltip("How long it takes the camera to tilt to its overshoot peak.")]
    private float postArrivalTiltDuration = 0.2f;

    [Tooltip("How many degrees the camera eases back from its peak tilt for the 'tease' before returning to the peak.")]
    private float postArrivalTiltTeaseAmount = 4f;

    [Tooltip("How long the tease ease-back and ease-back-to-peak each take.")]
    private float postArrivalTiltTeaseDuration = 0.12f;

    [Tooltip("Color of the light flash from the fade panel during the tilt tease.")]
    private Color postArrivalTeaseFlashColor = Color.white;

    [Tooltip("Peak alpha of the fade panel flash during the tilt tease.")]
    [Range(0f, 1f)]
    private float postArrivalTeaseFlashAlpha = 0.3f;

    [Tooltip("How long the door-swap flash takes to rise and fall right after the camera finishes zooming out.")]
    private float postArrivalDoorSwapFlashDuration = 0.2f;

    [Tooltip("How long it takes the camera to tilt back to normal after reaching its overshoot peak.")]
    private float postArrivalTiltReturnDuration = 0.22f;

    [Tooltip("How long the post-arrival zoom-in takes.")]
    private float postArrivalZoomDuration = 0.35f;

    [Tooltip("Peak alpha the final red fade should reach before being considered done, instead of fully opaque.")]
    [Range(0f, 1f)]
    private float postArrivalFinalFadeMaxAlpha = 1f;

    [Tooltip("How long the final red fade takes, independent of the zoom-in duration, so it can be much slower.")]
    private float postArrivalFinalFadeDuration = 1.6f;

    [Tooltip("How long to hold on the faded screen before the scene actually loads.")]
    private float postArrivalSceneLoadDelay = 0.3f;

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
        zoomInAmount = -0.5f;
        holdSpeedMultiplier = 4f;
        fadeDuration = 1f;
        shakeFrequency = 5f;
        shakeTiltAngle = 2.5f;
        fadeStartAlpha = 1f;
        fadeEndAlpha = 100f / 255f;
        startFadeOnEnable = true;
        pressAnyButtonTiltFrequency = 1.2f;
        pressAnyButtonGradientColorA = new Color(0.15f, 1f, 0.1f);
        pressAnyButtonGradientColorC = new Color(0.75f, 0.1f, 1f);
        pressAnyButtonGradientSpeed = 0.5f;
        pressAnyButtonScalePulseAmount = 0.08f;
        pressAnyButtonScaleNoiseSpeed = 0.7f;
        postArrivalWaitTime = 1f;
        postArrivalZoomAmount = 40f;
        postArrivalZoomOutAmount = 0.5f;
        postArrivalZoomOutDuration = 0.25f;
        postArrivalTiltAngle = 4f;
        postArrivalTiltOvershootExtra = 1.5f;
        postArrivalTiltDuration = 0.2f;
        postArrivalTiltTeaseAmount = 4f;
        postArrivalTiltTeaseDuration = 0.12f;
        postArrivalTeaseFlashColor = Color.white;
        postArrivalTeaseFlashAlpha = 0.3f;
        postArrivalDoorSwapFlashDuration = 0.2f;
        postArrivalTiltReturnDuration = 0.22f;
        postArrivalZoomDuration = 0.35f;
        postArrivalFinalFadeMaxAlpha = 1f;
        postArrivalFinalFadeDuration = 1.6f;
        postArrivalSceneLoadDelay = 0.3f;
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

    private static float EaseOutQuint(float t)
    {
        return 1f - Mathf.Pow(1f - t, 5f);
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

        if (pressAnyButtonAlpha > 0f)
        {
            ApplyPressAnyButtonGradient();
        }

        if (pressAnyButtonTargetAlpha > 0f)
        {
            float wave = Mathf.Sin(Time.time * pressAnyButtonTiltFrequency);
            float noise = Mathf.PerlinNoise(Time.time * pressAnyButtonScaleNoiseSpeed, 0.37f) * 2f - 1f;

            if (pressAnyButtonRect != null)
            {
                pressAnyButtonRect.localRotation = Quaternion.Euler(0f, 0f, wave * pressAnyButtonTiltAngle);
                float scale = 1f + noise * pressAnyButtonScalePulseAmount;
                pressAnyButtonRect.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            if (pressAnyButtonRect != null)
            {
                pressAnyButtonRect.localRotation = Quaternion.identity;
                pressAnyButtonRect.localScale = Vector3.one;
            }
        }

        if (Mathf.Approximately(pressAnyButtonAlpha, 0f) && pressAnyButtonTargetAlpha <= 0f)
        {
            pressAnyButtonText.text = string.Empty;
        }
    }

    private void ApplyPressAnyButtonGradient()
    {
        pressAnyButtonText.ForceMeshUpdate();
        TMP_TextInfo textInfo = pressAnyButtonText.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0)
        {
            return;
        }

        byte alphaByte = (byte)Mathf.RoundToInt(pressAnyButtonAlpha * 255f);
        float scroll = Time.time * pressAnyButtonGradientSpeed;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
            {
                continue;
            }

            float t = Mathf.PingPong((float)i / Mathf.Max(1, characterCount) + scroll, 1f);
            Color32 charColor = Color32.Lerp(pressAnyButtonGradientColorA, pressAnyButtonGradientColorC, t);
            charColor.a = alphaByte;

            Color32[] vertexColors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
            int vertexIndex = charInfo.vertexIndex;
            vertexColors[vertexIndex + 0] = charColor;
            vertexColors[vertexIndex + 1] = charColor;
            vertexColors[vertexIndex + 2] = charColor;
            vertexColors[vertexIndex + 3] = charColor;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.colors32 = meshInfo.colors32;
            pressAnyButtonText.UpdateGeometry(meshInfo.mesh, i);
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

    private void SwapDoorPositions()
    {
        if (doorclosedObject == null || openDoorObject == null)
        {
            return;
        }

        Vector3 closedPosition = doorclosedObject.localPosition;
        doorclosedObject.localPosition = openDoorObject.localPosition;
        openDoorObject.localPosition = closedPosition;
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

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(postArrivalTeaseFlashColor.r, postArrivalTeaseFlashColor.g, postArrivalTeaseFlashColor.b, 0f);
        }

        float halfFlashDuration = postArrivalDoorSwapFlashDuration * 0.5f;

        elapsed = 0f;
        while (elapsed < halfFlashDuration)
        {
            elapsed += Time.deltaTime;
            float flashT = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, halfFlashDuration));

            if (fadePanel != null)
            {
                Color flashColor = fadePanel.color;
                flashColor.a = Mathf.Lerp(0f, postArrivalTeaseFlashAlpha, flashT);
                fadePanel.color = flashColor;
            }

            yield return null;
        }

        SwapDoorPositions();

        elapsed = 0f;
        while (elapsed < halfFlashDuration)
        {
            elapsed += Time.deltaTime;
            float flashT = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, halfFlashDuration));

            if (fadePanel != null)
            {
                Color flashColor = fadePanel.color;
                flashColor.a = Mathf.Lerp(postArrivalTeaseFlashAlpha, 0f, flashT);
                fadePanel.color = flashColor;
            }

            yield return null;
        }

        if (fadePanel != null)
        {
            Color clearedFlashColor = fadePanel.color;
            clearedFlashColor.a = 0f;
            fadePanel.color = clearedFlashColor;
        }

        float peakTiltAngle = -(postArrivalTiltAngle + postArrivalTiltOvershootExtra);
        float teaseTiltAngle = -Mathf.Sign(peakTiltAngle) * postArrivalTiltTeaseAmount;

        elapsed = 0f;
        while (elapsed < postArrivalTiltDuration)
        {
            elapsed += Time.deltaTime;
            float tiltT = EaseOutQuint(Mathf.Clamp01(elapsed / Mathf.Max(0.001f, postArrivalTiltDuration)));
            cameraTransform.rotation = initialCameraRotation * Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, peakTiltAngle, tiltT));
            yield return null;
        }

        cameraTransform.rotation = initialCameraRotation * Quaternion.Euler(0f, 0f, peakTiltAngle);

        elapsed = 0f;
        while (elapsed < postArrivalTiltTeaseDuration)
        {
            elapsed += Time.deltaTime;
            float tiltT = EaseOutQuint(Mathf.Clamp01(elapsed / Mathf.Max(0.001f, postArrivalTiltTeaseDuration)));
            cameraTransform.rotation = initialCameraRotation * Quaternion.Euler(0f, 0f, Mathf.Lerp(peakTiltAngle, teaseTiltAngle, tiltT));
            yield return null;
        }

        cameraTransform.rotation = initialCameraRotation * Quaternion.Euler(0f, 0f, teaseTiltAngle);

        elapsed = 0f;
        while (elapsed < postArrivalTiltReturnDuration)
        {
            elapsed += Time.deltaTime;
            float tiltT = EaseInOutQuint(Mathf.Clamp01(elapsed / Mathf.Max(0.001f, postArrivalTiltReturnDuration)));
            cameraTransform.rotation = initialCameraRotation * Quaternion.Euler(0f, 0f, Mathf.Lerp(teaseTiltAngle, 0f, tiltT));
            yield return null;
        }

        cameraTransform.rotation = initialCameraRotation;

        Vector3 zoomInStartPosition = cameraTransform.position;
        Vector3 zoomInTargetPosition = postClickZoomTarget != null
            ? new Vector3(postClickZoomTarget.position.x, postClickZoomTarget.position.y, zoomInStartPosition.z)
            : zoomInStartPosition;

        StartCoroutine(FinalRedFadeRoutine());

        elapsed = 0f;
        while (elapsed < postArrivalZoomDuration)
        {
            elapsed += Time.deltaTime;
            float zoomT = EaseInOutQuint(Mathf.Clamp01(elapsed / Mathf.Max(0.001f, postArrivalZoomDuration)));

            if (activeCamera != null)
            {
                activeCamera.orthographicSize = Mathf.Lerp(zoomOutTargetSize, targetSize, zoomT);
            }

            cameraTransform.position = Vector3.Lerp(zoomInStartPosition, zoomInTargetPosition, zoomT);

            yield return null;
        }

        cameraTransform.position = zoomInTargetPosition;

        if (activeCamera != null)
        {
            activeCamera.orthographicSize = targetSize;
        }

        yield return new WaitForSeconds(postArrivalSceneLoadDelay);

        currentState = CameraState.Completed;
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FinalRedFadeRoutine()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        Material[] particleMaterials = GetParticleFadeMaterials();
        Color[] particleStartColors = new Color[particleMaterials.Length];

        for (int i = 0; i < particleMaterials.Length; i++)
        {
            particleStartColors[i] = particleMaterials[i].color;
        }

        foreach (ParticleSystem particleSystem in particlesToFadeOnRedTransition)
        {
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(1f, 0f, 0f, 0f);

        float fadeElapsed = 0f;
        while (fadeElapsed < postArrivalFinalFadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = Mathf.Clamp01(fadeElapsed / Mathf.Max(0.001f, postArrivalFinalFadeDuration));
            Color currentColor = fadePanel.color;
            currentColor.a = fadeT * postArrivalFinalFadeMaxAlpha;
            fadePanel.color = currentColor;

            for (int i = 0; i < particleMaterials.Length; i++)
            {
                Color particleColor = particleStartColors[i];
                particleColor.a = particleStartColors[i].a * (1f - fadeT);
                particleMaterials[i].color = particleColor;
            }

            yield return null;
        }

        Color finalColor = fadePanel.color;
        finalColor.a = postArrivalFinalFadeMaxAlpha;
        fadePanel.color = finalColor;
    }

    private Material[] GetParticleFadeMaterials()
    {
        if (particlesToFadeOnRedTransition == null || particlesToFadeOnRedTransition.Length == 0)
        {
            return System.Array.Empty<Material>();
        }

        System.Collections.Generic.List<Material> materials = new System.Collections.Generic.List<Material>();

        foreach (ParticleSystem particleSystem in particlesToFadeOnRedTransition)
        {
            if (particleSystem == null)
            {
                continue;
            }

            ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            if (particleRenderer != null)
            {
                materials.Add(particleRenderer.material);
            }
        }

        return materials.ToArray();
    }
}

