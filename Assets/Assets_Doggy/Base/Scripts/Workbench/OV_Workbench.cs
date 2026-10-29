using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OV_Workbench : MonoBehaviour
{
    [Serializable]
    public class ButtonAction
    {
        [Tooltip("Sound played when the button is clicked. If empty, the Default Click Sound is used.")]
        public AudioClip clickSound;
        [Tooltip("Optional: this canvas is activated after the animation and sits on top of the workbench.")]
        public Canvas targetCanvas;
        [Tooltip("Runs AFTER the button animation. Move the old OnClick calls here.")]
        public UnityEvent onActivated;
    }

    [Header("Interaction")]
    [SerializeField, Min(0f)] private float interactionRadius = 2.5f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player";

    [Header("Canvas")]
    [SerializeField] private Canvas workbenchCanvas;
    [SerializeField, Min(0f)] private float transitionDuration = 0.5f;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Workbench Buttons")]
    [SerializeField] private Button firstButton;
    [SerializeField] private Button secondButton;
    [SerializeField] private Button thirdButton;
    [SerializeField, Range(0f, 0.1f)] private float pulseAmount = 0.012f;
    [SerializeField, Min(0f)] private float pulseSpeed = 0.35f;
    [SerializeField, Min(1f)] private float hoverScale = 1.2f;
    [SerializeField, Min(0f)] private float buttonScaleSpeed = 10f;

    [Header("Button Actions")]
    [SerializeField] private ButtonAction firstAction;
    [SerializeField] private ButtonAction secondAction;
    [SerializeField] private ButtonAction thirdAction;

    [Header("Button Press Animation")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Used when a button's action has no sound assigned.")]
    [SerializeField] private AudioClip defaultClickSound;
    [SerializeField, Range(0f, 1f)] private float soundVolume = 1f;
    [Tooltip("Random pitch per click: 1 +/- this value (0.1 = +/-10%).")]
    [SerializeField, Range(0f, 0.5f)] private float pitchVariation = 0.1f;
    [SerializeField, Min(0f)] private float pressDuration = 0.3f;
    [Tooltip("Random tilt angle per press (x = min, y = max). Direction (left/right) is random too.")]
    [SerializeField] private Vector2 pressTiltAngleRange = new Vector2(5f, 12f);
    [SerializeField, Min(0f)] private float pressShakeAmount = 6f;
    [SerializeField, Min(0f)] private float pressMoveDistance = 14f;
    [SerializeField, Min(1f)] private float pressShakeCycles = 3f;

    private struct CanvasState
    {
        public bool WasActive;
        public int SortingOrder;
        public bool OverrideSorting;
    }

    private readonly List<ButtonVisual> buttonVisuals = new List<ButtonVisual>();
    private readonly Dictionary<Canvas, CanvasState> savedCanvases = new Dictionary<Canvas, CanvasState>();
    private RectTransform canvasRect;
    private CanvasGroup canvasGroup;
    private RectTransform[] canvasContent;
    private Vector2[] contentOpenPositions;
    private Vector2 contentOffset;
    private Coroutine transition;
    private Coroutine pressRoutine;
    private Transform player;
    private bool isOpen;
    private bool playerInRange;
    private bool controlsLocked;
    private float currentContentOffset = 1f;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisibility;

    private bool workbenchSortingRaised;
    private int workbenchOriginalSortingOrder;
    private bool workbenchOriginalOverrideSorting;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (workbenchCanvas == null)
        {
            Debug.LogError("[OV_Workbench] Assign the dedicated workbench Canvas in the Inspector.", this);
            enabled = false;
            return;
        }

        if (workbenchCanvas.gameObject == gameObject)
        {
            Debug.LogError("[OV_Workbench] Put this script on the workbench object, not on the Canvas it disables.", this);
            enabled = false;
            return;
        }

        canvasRect = workbenchCanvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("[OV_Workbench] The assigned Canvas has no RectTransform.", this);
            enabled = false;
            return;
        }

        canvasGroup = workbenchCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = workbenchCanvas.gameObject.AddComponent<CanvasGroup>();

        canvasContent = new RectTransform[workbenchCanvas.transform.childCount];
        contentOpenPositions = new Vector2[canvasContent.Length];
        for (int i = 0; i < canvasContent.Length; i++)
        {
            canvasContent[i] = workbenchCanvas.transform.GetChild(i) as RectTransform;
            if (canvasContent[i] != null)
                contentOpenPositions[i] = canvasContent[i].anchoredPosition;
        }

        RefreshContentOffset();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        SetContentOffset(1f);
        workbenchCanvas.gameObject.SetActive(false);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }

        ResolvePlayer();
        ConfigureButtons();
    }

    private void Update()
    {
        if (isOpen)
        {
            // While a button animation runs, ignore all input.
            if (pressRoutine == null)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    Close();

                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    PressButton(firstButton);
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    PressButton(secondButton);
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                    PressButton(thirdButton);
            }

            AnimateButtons();
            return;
        }

        UpdatePlayerRange();
        if (playerInRange && Input.GetKeyDown(interactionKey))
            Open();
    }

    private void OnDisable()
    {
        if (savedCanvases.Count > 0)
            RestoreOtherCanvases();

        if (workbenchSortingRaised)
            RestoreWorkbenchSorting();

        if (controlsLocked)
            RestorePlayerControls();
    }

    public void Open()
    {
        if (isOpen)
            return;

        if (workbenchCanvas == null || canvasRect == null || canvasGroup == null)
        {
            Debug.LogError("[OV_Workbench] Cannot open because the Canvas is not configured.", this);
            return;
        }

        if (transition != null)
            StopCoroutine(transition);

        LockPlayerControls();
        isOpen = true;

        // 1) Remember + deactivate every other canvas, 2) put the workbench on top.
        int topOrder = DeactivateOtherCanvases();
        RaiseWorkbenchSorting(topOrder + 1);

        workbenchCanvas.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        transition = StartCoroutine(AnimateCanvas(true));
    }

    public void Close()
    {
        if (!isOpen || pressRoutine != null)
            return;

        isOpen = false;

        if (transition != null)
            StopCoroutine(transition);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        RestoreButtonScales();

        // Bring back the previous canvases right away; the workbench is still on top while it fades out.
        RestoreOtherCanvases();

        transition = StartCoroutine(AnimateCanvas(false));
    }

    // ---------------------------------------------------------------- Canvas handling

    private int DeactivateOtherCanvases()
    {
        savedCanvases.Clear();
        int highestOrder = workbenchCanvas.sortingOrder;

        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null || canvas == workbenchCanvas || !canvas.isRootCanvas)
                continue;

            // Never disable a canvas that contains this script or the workbench canvas.
            if (transform.IsChildOf(canvas.transform) || workbenchCanvas.transform.IsChildOf(canvas.transform))
                continue;

            highestOrder = Mathf.Max(highestOrder, canvas.sortingOrder);

            savedCanvases[canvas] = new CanvasState
            {
                WasActive = canvas.gameObject.activeSelf,
                SortingOrder = canvas.sortingOrder,
                OverrideSorting = canvas.overrideSorting
            };

            if (canvas.gameObject.activeSelf)
                canvas.gameObject.SetActive(false);
        }

        return highestOrder;
    }

    private void RestoreOtherCanvases()
    {
        foreach (KeyValuePair<Canvas, CanvasState> pair in savedCanvases)
        {
            Canvas canvas = pair.Key;
            if (canvas == null)
                continue;

            canvas.overrideSorting = pair.Value.OverrideSorting;
            canvas.sortingOrder = pair.Value.SortingOrder;
            canvas.gameObject.SetActive(pair.Value.WasActive);
        }

        savedCanvases.Clear();
    }

    private void RaiseWorkbenchSorting(int order)
    {
        if (!workbenchSortingRaised)
        {
            workbenchOriginalSortingOrder = workbenchCanvas.sortingOrder;
            workbenchOriginalOverrideSorting = workbenchCanvas.overrideSorting;
            workbenchSortingRaised = true;
        }

        SetSorting(workbenchCanvas, order);
    }

    private void RestoreWorkbenchSorting()
    {
        if (!workbenchSortingRaised)
            return;

        workbenchCanvas.overrideSorting = workbenchOriginalOverrideSorting;
        workbenchCanvas.sortingOrder = workbenchOriginalSortingOrder;
        workbenchSortingRaised = false;
    }

    private static void SetSorting(Canvas canvas, int order)
    {
        if (!canvas.isRootCanvas)
            canvas.overrideSorting = true;

        canvas.sortingOrder = order;
    }

    private void ShowTargetCanvasOnTop(Canvas target)
    {
        if (target == null)
            return;

        target.gameObject.SetActive(true);
        SetSorting(target, workbenchCanvas.sortingOrder + 1);
    }

    // ---------------------------------------------------------------- Player

    private void UpdatePlayerRange()
    {
        if (player == null)
            return;

        playerInRange = Vector2.Distance(transform.position, player.position) <= interactionRadius;
    }

    private void ResolvePlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject == null)
        {
            Debug.LogWarning($"[OV_Workbench] No GameObject with tag '{playerTag}' was found.", this);
            return;
        }

        player = playerObject.transform;

        if (playerMovement == null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();

        if (playerMovement == null)
            Debug.LogWarning("[OV_Workbench] Assign the player's movement component to lock movement while open.", this);
    }

    private void LockPlayerControls()
    {
        if (controlsLocked)
            return;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        if (playerMovement != null)
            playerMovement.SetMovementInputLocked(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        controlsLocked = true;
    }

    private void RestorePlayerControls()
    {
        if (playerMovement != null)
            playerMovement.SetMovementInputLocked(false);

        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisibility;
        controlsLocked = false;
    }

    // ---------------------------------------------------------------- Open / close animation

    private void RefreshContentOffset()
    {
        // rect.height can be invalid while the canvas is inactive, so fall back to the screen height.
        float height = canvasRect.rect.height;
        if (height < 1f)
            height = Screen.height;

        contentOffset = Vector2.down * (height + 100f);
    }

    private IEnumerator AnimateCanvas(bool opening)
    {
        if (opening)
        {
            // Let Unity build the freshly activated canvas first, so that one-time
            // activation hitches don't eat into the animation time.
            yield return new WaitForEndOfFrame();
            RefreshContentOffset();
            SetContentOffset(currentContentOffset);
        }

        float startOffset = currentContentOffset;
        float targetOffset = opening ? 0f : 1f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = opening ? 1f : 0f;
        float startTime = Time.unscaledTime;

        // Wall-clock based: always takes exactly transitionDuration, no matter the frame spikes.
        while (true)
        {
            float t = transitionDuration > 0f
                ? Mathf.Clamp01((Time.unscaledTime - startTime) / transitionDuration)
                : 1f;
            float progress = Mathf.SmoothStep(0f, 1f, t);

            SetContentOffset(Mathf.Lerp(startOffset, targetOffset, progress));
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            if (t >= 1f)
                break;

            yield return null;
        }

        if (!opening)
        {
            workbenchCanvas.gameObject.SetActive(false);
            RestoreWorkbenchSorting();
            RestorePlayerControls();
        }

        transition = null;
    }

    private void SetContentOffset(float amount)
    {
        if (canvasContent == null)
            return;

        currentContentOffset = amount;
        for (int i = 0; i < canvasContent.Length; i++)
        {
            if (canvasContent[i] != null)
                canvasContent[i].anchoredPosition = contentOpenPositions[i] + contentOffset * amount;
        }
    }

    // ---------------------------------------------------------------- Buttons

    private void ConfigureButtons()
    {
        AddButton(firstButton, firstAction, 0f);
        AddButton(secondButton, secondAction, Mathf.PI * 0.67f);
        AddButton(thirdButton, thirdAction, Mathf.PI * 1.34f);
    }

    private void AddButton(Button button, ButtonAction action, float phase)
    {
        if (button == null)
            return;

        // Turn off the old OnClick entries from the Inspector. They would fire on every click
        // (even during the lock) and allow spamming. Their logic belongs in "On Activated".
        int persistentCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < persistentCount; i++)
            button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);

        ButtonVisual visual = new ButtonVisual(button, action, phase);
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();

        visual.AddPointerEvents(eventTrigger);
        button.onClick.AddListener(() => ActivateButton(visual));
        buttonVisuals.Add(visual);
    }

    // Keyboard shortcuts go through the button's onClick, so mouse and keys behave identically.
    private void PressButton(Button button)
    {
        if (button != null && button.interactable)
            button.onClick.Invoke();
    }

    private void ActivateButton(ButtonVisual visual)
    {
        // Only allow presses when fully open and no other press/transition is running.
        if (!isOpen || pressRoutine != null || transition != null)
            return;

        // Set immediately so a second click in the same frame can't slip through.
        pressRoutine = StartCoroutine(PressRoutine(visual));
    }

    private void PlayClickSound(ButtonAction action)
    {
        if (audioSource == null)
            return;

        AudioClip clip = (action != null && action.clickSound != null) ? action.clickSound : defaultClickSound;
        if (clip == null)
            return;

        // Slightly randomized pitch so repeated clicks don't sound identical.
        audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(clip, soundVolume);
    }

    private IEnumerator PressRoutine(ButtonVisual visual)
    {
        ButtonAction action = visual.Action;

        PlayClickSound(action);

        RectTransform rect = visual.Rect;
        if (rect != null && pressDuration > 0f)
        {
            // Remember the CURRENT position/rotation (button is on screen now).
            visual.BeginPress();

            // Random tilt strength and random direction for this press.
            float minTilt = Mathf.Min(pressTiltAngleRange.x, pressTiltAngleRange.y);
            float maxTilt = Mathf.Max(pressTiltAngleRange.x, pressTiltAngleRange.y);
            float tiltAngle = UnityEngine.Random.Range(minTilt, maxTilt);
            if (UnityEngine.Random.value < 0.5f)
                tiltAngle = -tiltAngle;

            float startTime = Time.unscaledTime;
            while (true)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - startTime) / pressDuration);
                float damping = 1f - t;

                // Tilt + horizontal shake fade out; the move goes out and comes back (0 -> 1 -> 0).
                float tilt = Mathf.Sin(t * pressShakeCycles * Mathf.PI * 2f) * tiltAngle * damping;
                float shake = Mathf.Sin(t * pressShakeCycles * Mathf.PI * 2f * 1.3f) * pressShakeAmount * damping;
                float move = Mathf.Sin(t * Mathf.PI) * pressMoveDistance;

                rect.anchoredPosition = visual.PressStartPosition + new Vector2(shake, -move);
                rect.localRotation = visual.PressStartRotation * Quaternion.Euler(0f, 0f, tilt);

                if (t >= 1f)
                    break;

                yield return null;
            }

            visual.ResetTransformOffsets();
        }

        // Animation done -> now run the button's own logic.
        if (action != null)
        {
            ShowTargetCanvasOnTop(action.targetCanvas);
            action.onActivated?.Invoke();
        }

        pressRoutine = null;
    }

    private void AnimateButtons()
    {
        float time = Time.unscaledTime * pulseSpeed * Mathf.PI * 2f;

        foreach (ButtonVisual visual in buttonVisuals)
        {
            if (visual.Button == null)
                continue;

            float pulse = visual.IsHovered ? 1f : 1f + Mathf.Sin(time + visual.Phase) * pulseAmount;
            float targetScale = pulse * (visual.IsHovered ? hoverScale : 1f);
            Vector3 target = visual.BaseScale * targetScale;
            visual.Button.transform.localScale = Vector3.Lerp(
                visual.Button.transform.localScale,
                target,
                1f - Mathf.Exp(-buttonScaleSpeed * Time.unscaledDeltaTime));
        }
    }

    private void RestoreButtonScales()
    {
        foreach (ButtonVisual visual in buttonVisuals)
        {
            if (visual.Button != null)
                visual.Button.transform.localScale = visual.BaseScale;

            visual.ResetTransformOffsets();
            visual.IsHovered = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    private sealed class ButtonVisual
    {
        public readonly Button Button;
        public readonly ButtonAction Action;
        public readonly RectTransform Rect;
        public readonly Vector3 BaseScale;
        public readonly float Phase;
        public bool IsHovered;

        public bool IsPressing;
        public Vector2 PressStartPosition;
        public Quaternion PressStartRotation;

        public ButtonVisual(Button button, ButtonAction action, float phase)
        {
            Button = button;
            Action = action;
            Rect = button.transform as RectTransform;
            BaseScale = button.transform.localScale;
            Phase = phase;
        }

        public void BeginPress()
        {
            if (Rect == null)
                return;

            PressStartPosition = Rect.anchoredPosition;
            PressStartRotation = Rect.localRotation;
            IsPressing = true;
        }

        public void ResetTransformOffsets()
        {
            if (!IsPressing || Rect == null)
                return;

            Rect.anchoredPosition = PressStartPosition;
            Rect.localRotation = PressStartRotation;
            IsPressing = false;
        }

        public void AddPointerEvents(EventTrigger eventTrigger)
        {
            if (eventTrigger.triggers == null)
                eventTrigger.triggers = new List<EventTrigger.Entry>();

            eventTrigger.triggers.Add(CreateEntry(EventTriggerType.PointerEnter, _ => IsHovered = true));
            eventTrigger.triggers.Add(CreateEntry(EventTriggerType.PointerExit, _ => IsHovered = false));
        }

        private static EventTrigger.Entry CreateEntry(EventTriggerType eventType, UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(callback);
            return entry;
        }
    }
}