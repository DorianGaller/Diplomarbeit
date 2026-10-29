using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OV_Workbench : MonoBehaviour
{
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

    private readonly List<ButtonVisual> buttonVisuals = new List<ButtonVisual>();
    private RectTransform canvasRect;
    private CanvasGroup canvasGroup;
    private RectTransform[] canvasContent;
    private Vector2[] contentOpenPositions;
    private Vector2 contentOffset;
    private Coroutine transition;
    private Transform player;
    private bool isOpen;
    private bool playerInRange;
    private bool controlsLocked;
    private float currentContentOffset = 1f;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisibility;

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

        contentOffset = Vector2.down * (canvasRect.rect.height + 100f);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        SetContentOffset(1f);
        workbenchCanvas.gameObject.SetActive(false);

        ResolvePlayer();
        ConfigureButtons();
    }

    private void Update()
    {
        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                PressButton(firstButton);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                PressButton(secondButton);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                PressButton(thirdButton);

            AnimateButtons();
            return;
        }

        UpdatePlayerRange();
        if (playerInRange && Input.GetKeyDown(interactionKey))
            Open();
    }

    private void OnDisable()
    {
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
        workbenchCanvas.gameObject.SetActive(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        transition = StartCoroutine(AnimateCanvas(true));
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (transition != null)
            StopCoroutine(transition);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        RestoreButtonScales();
        transition = StartCoroutine(AnimateCanvas(false));
    }

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

    private IEnumerator AnimateCanvas(bool opening)
    {
        float startOffset = currentContentOffset;
        float targetOffset = opening ? 0f : 1f;
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = opening ? 1f : 0f;
        float elapsed = 0f;

        if (transitionDuration <= 0f)
        {
            SetContentOffset(targetOffset);
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
                SetContentOffset(Mathf.Lerp(startOffset, targetOffset, progress));
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }

            SetContentOffset(targetOffset);
            canvasGroup.alpha = targetAlpha;
        }

        if (!opening)
        {
            workbenchCanvas.gameObject.SetActive(false);
            RestorePlayerControls();
        }

        transition = null;
    }

    private void ConfigureButtons()
    {
        AddButton(firstButton, 0f);
        AddButton(secondButton, Mathf.PI * 0.67f);
        AddButton(thirdButton, Mathf.PI * 1.34f);
    }

    private void PressButton(Button button)
    {
        if (button != null && button.interactable)
            button.onClick.Invoke();
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

    private void AddButton(Button button, float phase)
    {
        if (button == null)
            return;

        ButtonVisual visual = new ButtonVisual(button, phase);
        EventTrigger eventTrigger = button.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();

        visual.AddPointerEvents(eventTrigger);
        buttonVisuals.Add(visual);
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
        public readonly Vector3 BaseScale;
        public readonly float Phase;
        public bool IsHovered;

        public ButtonVisual(Button button, float phase)
        {
            Button = button;
            BaseScale = button.transform.localScale;
            Phase = phase;
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
