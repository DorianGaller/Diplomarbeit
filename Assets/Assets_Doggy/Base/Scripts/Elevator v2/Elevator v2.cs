using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Elevatorv2 : MonoBehaviour
{
    [Serializable]
    private sealed class SceneEntry
    {
        [SerializeField] private Button sceneButton;
        [SerializeField] private string sceneName;

        public Button SceneButton => sceneButton;
        public string SceneName => sceneName;
    }

    [Serializable]
    private sealed class InformationEntry
    {
        [SerializeField] private Button informationButton;
        [TextArea, SerializeField] private string informationText;

        public Button InformationButton => informationButton;
        public string InformationText => informationText;
    }

    private struct CanvasState
    {
        public bool WasActive;
        public int SortingOrder;
        public bool OverrideSorting;
    }

    private sealed class ButtonBinding
    {
        public Button Button;
        public UnityAction Listener;
    }

    [Header("Interaction")]
    [SerializeField, Min(0f)] private float interactionRadius = 4f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press F to interact";
    [SerializeField, Min(0f)] private float promptFadeDuration = 0.2f;
    [SerializeField] private GameObject interactionCanvas;
    [SerializeField] private TMP_Text interactionPrompt;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Elevator Canvas")]
    [SerializeField] private Canvas elevatorCanvas;
    [SerializeField, Min(0f)] private float transitionDuration = 0.5f;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button slideBackButton;
    [SerializeField] private TMP_Text slideBackLabel;
    [SerializeField] private string slideBackText = "Slide Back";
    [SerializeField, Range(0.5f, 0.98f)] private float buttonPressScale = 0.88f;
    [SerializeField, Min(0.01f)] private float buttonPressDuration = 0.16f;

    [Header("Information View")]
    [SerializeField] private GameObject frontPanelObject;
    [SerializeField] private GameObject backPanelObject;
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform informationPanel;
    [SerializeField] private TMP_Text informationText;
    [SerializeField] private Vector2 mainPanelSlideOffset = new Vector2(-450f, 0f);
    [SerializeField] private Vector2 informationPanelStartOffset = new Vector2(900f, 0f);
    [SerializeField] private Vector2 frontPanelSlideOffset = new Vector2(-450f, 0f);
    [SerializeField] private Vector2 backPanelSlideOffset = new Vector2(450f, 0f);
    [Tooltip("Normalized position of the printed Zurueck button on the back panel sprite.")]
    [SerializeField] private Vector2 slideBackButtonSpritePosition = new Vector2(0.32f, 0.08f);
    [SerializeField] private SceneEntry[] sceneEntries = new SceneEntry[5];
    [SerializeField] private InformationEntry[] informationEntries = new InformationEntry[5];

    private readonly Dictionary<Canvas, CanvasState> savedCanvases = new Dictionary<Canvas, CanvasState>();
    private readonly List<ButtonBinding> buttonBindings = new List<ButtonBinding>();
    private readonly List<RectTransform> canvasContent = new List<RectTransform>();
    private readonly List<Vector2> contentOpenPositions = new List<Vector2>();
    private readonly List<Transform> spriteCanvasContent = new List<Transform>();
    private readonly List<Vector3> spriteContentOpenPositions = new List<Vector3>();
    private readonly List<bool> informationButtonInteractability = new List<bool>();
    private readonly Dictionary<Button, Coroutine> buttonPressRoutines = new Dictionary<Button, Coroutine>();
    private readonly HashSet<Button> pressAnimatedButtons = new HashSet<Button>();

    private Transform player;
    private ElevatorInteractable sceneLoader;
    private RectTransform canvasRect;
    private Image frontPanelImage;
    private Image backPanelImage;
    private RectTransform slideBackButtonRect;
    private CanvasGroup slideBackButtonGroup;
    private CanvasGroup elevatorCanvasGroup;
    private CanvasGroup interactionCanvasGroup;
    private Vector2 contentOffset;
    private Vector2 mainPanelOpenPosition;
    private Vector2 informationPanelOpenPosition;
    private Vector2 frontPanelOpenPosition;
    private Vector2 backPanelOpenPosition;
    private Coroutine transitionRoutine;
    private Coroutine panelRoutine;
    private Coroutine promptFadeRoutine;
    private bool playerInRange;
    private bool isOpen;
    private bool isClosing;
    private bool informationViewOpen;
    private bool cursorStateSaved;
    private bool elevatorSortingRaised;
    private int elevatorOriginalSortingOrder;
    private bool elevatorOriginalOverrideSorting;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisibility;
    private bool playerLookupWarningLogged;
    private bool movementLookupWarningLogged;
    private bool controlsLocked;
    private float currentContentOffset = 1f;

    private void Awake()
    {
        if (elevatorCanvas == null)
        {
            Debug.LogError("[Elevatorv2] Assign the Elevator Canvas V2 in the Inspector.", this);
            enabled = false;
            return;
        }

        canvasRect = elevatorCanvas.transform as RectTransform;
        if (canvasRect == null)
        {
            Debug.LogError("[Elevatorv2] The assigned Elevator Canvas has no RectTransform.", this);
            enabled = false;
            return;
        }

        elevatorCanvasGroup = elevatorCanvas.GetComponent<CanvasGroup>();
        if (elevatorCanvasGroup == null)
            elevatorCanvasGroup = elevatorCanvas.gameObject.AddComponent<CanvasGroup>();

        if (mainPanel != null)
            mainPanelOpenPosition = mainPanel.anchoredPosition;
        if (informationPanel != null)
        {
            informationPanelOpenPosition = informationPanel.anchoredPosition;
            informationPanel.anchoredPosition = informationPanelOpenPosition + informationPanelStartOffset;
        }

        ApplyPanelImageOrder();
        if (frontPanelImage != null)
            frontPanelOpenPosition = frontPanelImage.rectTransform.anchoredPosition;
        if (backPanelImage != null)
            backPanelOpenPosition = backPanelImage.rectTransform.anchoredPosition;
        SetupSlideBackButton();

        for (int i = 0; i < elevatorCanvas.transform.childCount; i++)
        {
            Transform child = elevatorCanvas.transform.GetChild(i);

            if (child is RectTransform rectTransform)
            {
                canvasContent.Add(rectTransform);
                contentOpenPositions.Add(rectTransform.anchoredPosition);
            }
            else if ((frontPanelObject != null && child.gameObject == frontPanelObject) ||
                     (backPanelObject != null && child.gameObject == backPanelObject))
            {
                spriteCanvasContent.Add(child);
                spriteContentOpenPositions.Add(child.localPosition);
            }
        }

        RefreshContentOffset();
        SetContentOffset(1f);
        elevatorCanvasGroup.alpha = 0f;
        elevatorCanvasGroup.interactable = false;
        elevatorCanvasGroup.blocksRaycasts = false;
        elevatorCanvas.gameObject.SetActive(false);

        if (interactionCanvas != null)
        {
            interactionCanvasGroup = interactionCanvas.GetComponent<CanvasGroup>();
            if (interactionCanvasGroup == null)
                interactionCanvasGroup = interactionCanvas.AddComponent<CanvasGroup>();

            interactionCanvasGroup.alpha = 0f;
            interactionCanvasGroup.interactable = false;
            interactionCanvasGroup.blocksRaycasts = false;
            interactionCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("[Elevatorv2] Assign the Interaction Canvas in the Inspector.", this);
        }

        Canvas interactionRootCanvas = interactionCanvas != null
            ? interactionCanvas.GetComponentInParent<Canvas>(true)
            : null;
        if (interactionRootCanvas != null && interactionRootCanvas == elevatorCanvas.rootCanvas)
            Debug.LogError("[Elevatorv2] Use separate root canvases for the prompt and elevator UI.", this);

        if (interactionPrompt != null)
            interactionPrompt.text = promptText;
        else
            Debug.LogError("[Elevatorv2] Assign the interaction prompt TMP_Text in the Inspector.", this);

        if (informationPanel != null)
            informationPanel.gameObject.SetActive(false);
        else
            Debug.LogError("[Elevatorv2] Assign the information panel in the Inspector.", this);

        if (mainPanel == null || informationText == null)
            Debug.LogError("[Elevatorv2] Assign the main panel and information TMP_Text in the Inspector.", this);
        if (closeButton == null)
            Debug.LogError("[Elevatorv2] Assign the dedicated close button in the Inspector.", this);
        if (slideBackButton == null)
            Debug.LogError("[Elevatorv2] Assign the Slide Back button in the Inspector.", this);

        if (slideBackButton != null)
        {
            slideBackButton.gameObject.SetActive(false);
            slideBackButton.onClick.AddListener(SlideBack);
            buttonBindings.Add(new ButtonBinding { Button = slideBackButton, Listener = SlideBack });
            AddButtonPressAnimation(slideBackButton);
        }

        if (slideBackLabel != null)
            slideBackLabel.text = slideBackText;

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
            buttonBindings.Add(new ButtonBinding { Button = closeButton, Listener = Close });
            AddButtonPressAnimation(closeButton);
        }

        ConfigureSceneButtons();
        ConfigureInformationButtons();
        ResolvePlayer();
        DisableLegacyInteractionHandlers();
    }

    private void Update()
    {
        if (isOpen || isClosing)
        {
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
                Close();

            return;
        }

        if (player == null)
        {
            ResolvePlayer();
            if (player == null)
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    StartPromptFade(false);
                }

                return;
            }
        }

        UpdatePlayerRange();
        if (playerInRange && Input.GetKeyDown(interactionKey))
            Open();
    }

    private void OnDisable()
    {
        if (isOpen || isClosing || savedCanvases.Count > 0 || cursorStateSaved)
            RestoreStateImmediately();

        StopPromptFade();
        if (interactionCanvas != null)
        {
            if (interactionCanvasGroup != null)
                interactionCanvasGroup.alpha = 0f;
            interactionCanvas.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        foreach (ButtonBinding binding in buttonBindings)
        {
            if (binding.Button != null)
                binding.Button.onClick.RemoveListener(binding.Listener);
        }

        foreach (Coroutine routine in buttonPressRoutines.Values)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        buttonPressRoutines.Clear();
    }

    private void OnValidate()
    {
        if (sceneEntries == null || sceneEntries.Length != 5)
            Array.Resize(ref sceneEntries, 5);
        if (informationEntries == null || informationEntries.Length != 5)
            Array.Resize(ref informationEntries, 5);
    }

    public void Open()
    {
        if (isOpen || isClosing)
            return;

        if (!playerInRange)
            return;

        if (elevatorCanvas == null || elevatorCanvasGroup == null)
        {
            Debug.LogError("[Elevatorv2] Cannot open because the Elevator Canvas is not configured.", this);
            return;
        }

        isOpen = true;
        isClosing = false;
        ResetInformationView();
        LockPlayerControls();
        UnlockCursorForUI();

        int topOrder = DeactivateOtherCanvases();
        RaiseElevatorSorting(topOrder + 1);
        elevatorCanvas.gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();

        elevatorCanvasGroup.interactable = false;
        elevatorCanvasGroup.blocksRaycasts = false;
        StartPromptFade(false);
        StartCanvasTransition(true);
    }

    public void Close()
    {
        if (!isOpen || isClosing)
            return;

        isOpen = false;
        isClosing = true;

        if (panelRoutine != null)
        {
            StopCoroutine(panelRoutine);
            panelRoutine = null;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        elevatorCanvasGroup.interactable = false;
        elevatorCanvasGroup.blocksRaycasts = false;
        StartCanvasTransition(false);
    }

    public void CloseFromButton() => Close();

    public void SlideBack()
    {
        if (!isOpen || !informationViewOpen || panelRoutine != null)
            return;

        panelRoutine = StartCoroutine(AnimateInformationView(false));
    }

    private void ConfigureSceneButtons()
    {
        if (sceneEntries.Length != 5)
            Debug.LogError("[Elevatorv2] Exactly five scene entries must be configured.", this);

        for (int i = 0; i < sceneEntries.Length; i++)
        {
            SceneEntry entry = sceneEntries[i];
            if (entry == null)
            {
                Debug.LogError($"[Elevatorv2] Scene entry {i + 1} is not configured.", this);
                continue;
            }

            if (entry.SceneButton != null)
            {
                UnityAction listener = () => LoadScene(entry);
                entry.SceneButton.onClick.AddListener(listener);
                buttonBindings.Add(new ButtonBinding { Button = entry.SceneButton, Listener = listener });
                AddButtonPressAnimation(entry.SceneButton);
            }
            else
            {
                Debug.LogError($"[Elevatorv2] Scene button {i + 1} is not assigned.", this);
            }

        }
    }

    private void AddButtonPressAnimation(Button button)
    {
        if (button == null || !pressAnimatedButtons.Add(button))
            return;

        UnityAction listener = () =>
        {
            if (buttonPressRoutines.TryGetValue(button, out Coroutine existingRoutine) &&
                existingRoutine != null)
            {
                StopCoroutine(existingRoutine);
            }

            buttonPressRoutines[button] = StartCoroutine(AnimateButtonPress(button));
        };

        button.onClick.AddListener(listener);
        buttonBindings.Add(new ButtonBinding { Button = button, Listener = listener });
    }

    private IEnumerator AnimateButtonPress(Button button)
    {
        if (button == null)
            yield break;

        Transform buttonTransform = button.transform;
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 pressedScale = originalScale * buttonPressScale;
        float halfDuration = buttonPressDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = halfDuration > 0f ? Mathf.Clamp01(elapsed / halfDuration) : 1f;
            buttonTransform.localScale = Vector3.Lerp(originalScale, pressedScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = halfDuration > 0f ? Mathf.Clamp01(elapsed / halfDuration) : 1f;
            buttonTransform.localScale = Vector3.Lerp(pressedScale, originalScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        buttonTransform.localScale = originalScale;
        buttonPressRoutines.Remove(button);
    }

    private void ApplyPanelImageOrder()
    {
        if (frontPanelObject == null || backPanelObject == null)
        {
            Debug.LogError("[Elevatorv2] Assign both front and back panel sprite objects in the Inspector.", this);
            return;
        }

        SpriteRenderer frontRenderer = frontPanelObject.GetComponent<SpriteRenderer>();
        SpriteRenderer backRenderer = backPanelObject.GetComponent<SpriteRenderer>();
        if (frontRenderer == null || frontRenderer.sprite == null ||
            backRenderer == null || backRenderer.sprite == null)
        {
            Debug.LogError("[Elevatorv2] Both panel objects must have a SpriteRenderer with an assigned sprite.", this);
            return;
        }

        frontPanelImage = CreateCanvasPanelImage(frontPanelObject, frontRenderer, "Front Panel Image");
        backPanelImage = CreateCanvasPanelImage(backPanelObject, backRenderer, "Back Panel Image");

        if (frontPanelImage == null || backPanelImage == null)
            return;

        frontPanelImage.transform.SetAsFirstSibling();
        backPanelImage.transform.SetAsFirstSibling();
    }

    private void SetupSlideBackButton()
    {
        if (slideBackButton == null || frontPanelImage == null || backPanelImage == null)
            return;

        slideBackButtonRect = slideBackButton.GetComponent<RectTransform>();
        if (slideBackButtonRect == null)
        {
            Debug.LogError("[Elevatorv2] The Slide Back button requires a RectTransform.", this);
            return;
        }

        slideBackButtonRect.SetParent(backPanelImage.rectTransform, false);
        slideBackButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        slideBackButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        slideBackButtonRect.pivot = new Vector2(0.5f, 0.5f);
        slideBackButtonRect.localScale = Vector3.one;

        slideBackButtonGroup = slideBackButton.GetComponent<CanvasGroup>();
        if (slideBackButtonGroup == null)
            slideBackButtonGroup = slideBackButton.gameObject.AddComponent<CanvasGroup>();

        slideBackButtonGroup.alpha = 1f;
        slideBackButtonGroup.interactable = false;
        slideBackButtonGroup.blocksRaycasts = false;
        slideBackButtonRect.anchoredPosition = GetSlideBackButtonPosition();
        slideBackButton.gameObject.SetActive(false);
    }

    private Vector2 GetSlideBackButtonPosition()
    {
        RectTransform backRect = backPanelImage.rectTransform;
        return Vector2.Scale(
            slideBackButtonSpritePosition - new Vector2(0.5f, 0.5f),
            backRect.rect.size);
    }

    private Image CreateCanvasPanelImage(GameObject source, SpriteRenderer sourceRenderer, string imageName)
    {
        GameObject imageObject = new GameObject(imageName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.layer = elevatorCanvas.gameObject.layer;
        imageObject.transform.SetParent(elevatorCanvas.transform, false);

        RectTransform imageRect = (RectTransform)imageObject.transform;
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = source.transform.localPosition;
        imageRect.localRotation = source.transform.localRotation;
        imageRect.sizeDelta = sourceRenderer.sprite.rect.size;
        imageRect.localScale = source.transform.localScale / sourceRenderer.sprite.pixelsPerUnit;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sourceRenderer.sprite;
        image.color = sourceRenderer.color;
        image.preserveAspect = true;
        image.raycastTarget = false;
        sourceRenderer.enabled = false;
        return image;
    }

    private void ConfigureInformationButtons()
    {
        if (informationEntries.Length != 5)
            Debug.LogError("[Elevatorv2] Exactly five information entries must be configured.", this);

        for (int i = 0; i < informationEntries.Length; i++)
        {
            InformationEntry entry = informationEntries[i];
            informationButtonInteractability.Add(entry != null && entry.InformationButton != null
                ? entry.InformationButton.interactable
                : false);

            if (entry == null)
            {
                Debug.LogError($"[Elevatorv2] Information entry {i + 1} is not configured.", this);
                continue;
            }

            if (entry.InformationButton != null)
            {
                int entryIndex = i;
                UnityAction listener = () => OpenInformationView(entryIndex);
                entry.InformationButton.onClick.AddListener(listener);
                buttonBindings.Add(new ButtonBinding { Button = entry.InformationButton, Listener = listener });
                AddButtonPressAnimation(entry.InformationButton);
            }
            else
            {
                Debug.LogError($"[Elevatorv2] Information button {i + 1} is not assigned.", this);
            }

            if (string.IsNullOrWhiteSpace(entry.InformationText))
                Debug.LogWarning($"[Elevatorv2] Information text {i + 1} is empty.", this);
        }
    }

    private void OpenInformationView(int entryIndex)
    {
        if (!isOpen || transitionRoutine != null || panelRoutine != null ||
            informationViewOpen || entryIndex < 0 || entryIndex >= informationEntries.Length)
            return;

        InformationEntry entry = informationEntries[entryIndex];
        if (entry == null || mainPanel == null || informationPanel == null || informationText == null)
        {
            Debug.LogError("[Elevatorv2] Assign the main panel, information panel, and information text.", this);
            return;
        }

        informationText.text = entry.InformationText;
        informationPanel.gameObject.SetActive(true);
        if (slideBackButton != null)
        {
            slideBackButtonGroup.alpha = 1f;
            slideBackButtonGroup.interactable = false;
            slideBackButtonGroup.blocksRaycasts = false;
            slideBackButton.gameObject.SetActive(true);
        }

        informationViewOpen = true;
        for (int i = 0; i < informationEntries.Length; i++)
        {
            if (informationEntries[i] != null && informationEntries[i].InformationButton != null)
                informationEntries[i].InformationButton.interactable = false;
        }

        panelRoutine = StartCoroutine(AnimateInformationView(true));
    }

    private IEnumerator AnimateInformationView(bool opening)
    {
        Vector2 mainStart = mainPanel.anchoredPosition;
        Vector2 informationStart = informationPanel.anchoredPosition;
        Vector2 mainTarget = opening
            ? mainPanelOpenPosition + mainPanelSlideOffset
            : mainPanelOpenPosition;
        Vector2 informationTarget = opening
            ? informationPanelOpenPosition
            : informationPanelOpenPosition + informationPanelStartOffset;
        Vector2 frontStart = frontPanelImage != null
            ? frontPanelImage.rectTransform.anchoredPosition
            : Vector2.zero;
        Vector2 backStart = backPanelImage != null
            ? backPanelImage.rectTransform.anchoredPosition
            : Vector2.zero;
        Vector2 frontTarget = frontPanelOpenPosition + (opening ? frontPanelSlideOffset : Vector2.zero);
        Vector2 backTarget = backPanelOpenPosition + (opening ? backPanelSlideOffset : Vector2.zero);
        if (slideBackButtonGroup != null)
        {
            slideBackButtonGroup.interactable = false;
            slideBackButtonGroup.blocksRaycasts = false;
        }

        float startTime = Time.unscaledTime;

        while (true)
        {
            float t = transitionDuration > 0f
                ? Mathf.Clamp01((Time.unscaledTime - startTime) / transitionDuration)
                : 1f;
            float progress = Mathf.SmoothStep(0f, 1f, t);

            mainPanel.anchoredPosition = Vector2.Lerp(mainStart, mainTarget, progress);
            informationPanel.anchoredPosition = Vector2.Lerp(informationStart, informationTarget, progress);
            if (frontPanelImage != null)
                frontPanelImage.rectTransform.anchoredPosition = Vector2.Lerp(frontStart, frontTarget, progress);
            if (backPanelImage != null)
                backPanelImage.rectTransform.anchoredPosition = Vector2.Lerp(backStart, backTarget, progress);

            if (t >= 1f)
                break;

            yield return null;
        }

        mainPanel.anchoredPosition = mainTarget;
        informationPanel.anchoredPosition = informationTarget;
        if (frontPanelImage != null)
            frontPanelImage.rectTransform.anchoredPosition = frontTarget;
        if (backPanelImage != null)
            backPanelImage.rectTransform.anchoredPosition = backTarget;
        if (opening && slideBackButtonGroup != null)
        {
            slideBackButtonGroup.interactable = true;
            slideBackButtonGroup.blocksRaycasts = true;
        }

        if (!opening)
        {
            informationPanel.gameObject.SetActive(false);
            if (slideBackButton != null)
            {
                if (slideBackButtonGroup != null)
                {
                    slideBackButtonGroup.alpha = 1f;
                    slideBackButtonGroup.interactable = false;
                    slideBackButtonGroup.blocksRaycasts = false;
                }
                slideBackButton.gameObject.SetActive(false);
            }

            informationViewOpen = false;
            RestoreInformationButtonInteractability();
        }

        panelRoutine = null;
    }

    private void LoadScene(SceneEntry entry)
    {
        if (!isOpen || transitionRoutine != null || panelRoutine != null)
            return;

        if (string.IsNullOrWhiteSpace(entry.SceneName))
        {
            Debug.LogError("[Elevatorv2] A scene name is missing from the selected scene entry.", this);
            return;
        }

        if (sceneLoader == null)
            sceneLoader = FindAnyObjectByType<ElevatorInteractable>(FindObjectsInactive.Include);

        if (sceneLoader == null)
        {
            Debug.LogError("[Elevatorv2] No ElevatorInteractable was found to preserve elevator entry behavior.", this);
            return;
        }

        sceneLoader.LoadScene(entry.SceneName);
    }

    private void UpdatePlayerRange()
    {
        if (player == null)
        {
            if (playerInRange)
            {
                playerInRange = false;
                StartPromptFade(false);
            }

            return;
        }

        bool inRange = Vector2.Distance(transform.position, player.position) <= interactionRadius;
        if (inRange == playerInRange)
            return;

        playerInRange = inRange;
        StartPromptFade(playerInRange);
    }

    private void ResolvePlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject == null)
        {
            if (!playerLookupWarningLogged)
            {
                Debug.LogWarning($"[Elevatorv2] No GameObject with tag '{playerTag}' was found.", this);
                playerLookupWarningLogged = true;
            }
            return;
        }

        playerLookupWarningLogged = false;
        player = playerObject.transform;

        if (playerMovement == null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();

        if (playerMovement == null && !movementLookupWarningLogged)
        {
            Debug.LogWarning("[Elevatorv2] Assign the player's PlayerMovement component to lock movement while the elevator UI is open.", this);
            movementLookupWarningLogged = true;
        }
        else if (playerMovement != null)
        {
            movementLookupWarningLogged = false;
        }
    }

    private void LockPlayerControls()
    {
        if (controlsLocked)
            return;

        if (playerMovement == null)
            ResolvePlayer();

        if (playerMovement != null)
            playerMovement.SetMovementInputLocked(true);

        controlsLocked = true;
    }

    private void RestorePlayerControls()
    {
        if (!controlsLocked)
            return;

        if (playerMovement != null)
            playerMovement.SetMovementInputLocked(false);

        controlsLocked = false;
    }

    private void DisableLegacyInteractionHandlers()
    {
        ElevatorInteractable[] legacyHandlers = GetComponents<ElevatorInteractable>();
        foreach (ElevatorInteractable handler in legacyHandlers)
        {
            handler.ClosePanel();
            handler.enabled = false;
        }
    }

    private void UnlockCursorForUI()
    {
        if (cursorStateSaved)
            return;

        previousCursorLockMode = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;
        cursorStateSaved = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestoreCursorState()
    {
        if (!cursorStateSaved)
            return;

        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisibility;
        cursorStateSaved = false;
    }

    private int DeactivateOtherCanvases()
    {
        savedCanvases.Clear();
        int highestOrder = elevatorCanvas.sortingOrder;
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas == null || canvas == elevatorCanvas || !canvas.isRootCanvas)
                continue;

            if (transform.IsChildOf(canvas.transform) ||
                elevatorCanvas.transform.IsChildOf(canvas.transform) ||
                (interactionCanvas != null && interactionCanvas.transform.IsChildOf(canvas.transform)))
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

    private void RaiseElevatorSorting(int order)
    {
        if (!elevatorSortingRaised)
        {
            elevatorOriginalSortingOrder = elevatorCanvas.sortingOrder;
            elevatorOriginalOverrideSorting = elevatorCanvas.overrideSorting;
            elevatorSortingRaised = true;
        }

        if (!elevatorCanvas.isRootCanvas)
            elevatorCanvas.overrideSorting = true;

        elevatorCanvas.sortingOrder = order;
    }

    private void RestoreElevatorSorting()
    {
        if (!elevatorSortingRaised || elevatorCanvas == null)
            return;

        elevatorCanvas.overrideSorting = elevatorOriginalOverrideSorting;
        elevatorCanvas.sortingOrder = elevatorOriginalSortingOrder;
        elevatorSortingRaised = false;
    }

    private void StartCanvasTransition(bool opening)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(AnimateCanvas(opening));
    }

    private IEnumerator AnimateCanvas(bool opening)
    {
        if (opening)
        {
            yield return new WaitForEndOfFrame();
            RefreshContentOffset();
            SetContentOffset(currentContentOffset);
        }

        float startOffset = currentContentOffset;
        float targetOffset = opening ? 0f : 1f;
        float startAlpha = elevatorCanvasGroup.alpha;
        float targetAlpha = opening ? 1f : 0f;
        float startTime = Time.unscaledTime;

        while (true)
        {
            float t = transitionDuration > 0f
                ? Mathf.Clamp01((Time.unscaledTime - startTime) / transitionDuration)
                : 1f;
            float progress = Mathf.SmoothStep(0f, 1f, t);

            SetContentOffset(Mathf.Lerp(startOffset, targetOffset, progress));
            elevatorCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            if (t >= 1f)
                break;

            yield return null;
        }

        if (opening)
        {
            elevatorCanvasGroup.interactable = true;
            elevatorCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            elevatorCanvas.gameObject.SetActive(false);
            ResetInformationView();
            RestoreOtherCanvases();
            RestoreElevatorSorting();
            RestoreCursorState();
            RestorePlayerControls();
            isClosing = false;
            UpdatePlayerRange();
            if (playerInRange)
                StartPromptFade(true);
        }

        transitionRoutine = null;
    }

    private void RefreshContentOffset()
    {
        float height = canvasRect.rect.height;
        if (height < 1f)
            height = Screen.height;

        contentOffset = Vector2.down * (height + 100f);
    }

    private void SetContentOffset(float amount)
    {
        currentContentOffset = amount;
        for (int i = 0; i < canvasContent.Count; i++)
        {
            if (canvasContent[i] != null)
                canvasContent[i].anchoredPosition = contentOpenPositions[i] + contentOffset * amount;
        }

        for (int i = 0; i < spriteCanvasContent.Count; i++)
        {
            if (spriteCanvasContent[i] != null)
                spriteCanvasContent[i].localPosition = spriteContentOpenPositions[i] + (Vector3)(contentOffset * amount);
        }
    }

    private void StartPromptFade(bool visible)
    {
        if (interactionCanvas == null || interactionCanvasGroup == null)
            return;

        if (promptFadeRoutine != null)
            StopCoroutine(promptFadeRoutine);

        if (visible)
            interactionCanvas.SetActive(true);

        promptFadeRoutine = StartCoroutine(FadePrompt(visible));
    }

    private IEnumerator FadePrompt(bool visible)
    {
        float startAlpha = interactionCanvasGroup.alpha;
        float targetAlpha = visible ? 1f : 0f;
        float startTime = Time.unscaledTime;

        while (true)
        {
            float t = promptFadeDuration > 0f
                ? Mathf.Clamp01((Time.unscaledTime - startTime) / promptFadeDuration)
                : 1f;

            interactionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0f, 1f, t));
            if (t >= 1f)
                break;

            yield return null;
        }

        if (!visible)
            interactionCanvas.SetActive(false);

        promptFadeRoutine = null;
    }

    private void StopPromptFade()
    {
        if (promptFadeRoutine == null)
            return;

        StopCoroutine(promptFadeRoutine);
        promptFadeRoutine = null;
    }

    private void ResetInformationView()
    {
        if (panelRoutine != null)
        {
            StopCoroutine(panelRoutine);
            panelRoutine = null;
        }

        if (mainPanel != null)
            mainPanel.anchoredPosition = mainPanelOpenPosition;
        if (frontPanelImage != null)
            frontPanelImage.rectTransform.anchoredPosition = frontPanelOpenPosition;
        if (backPanelImage != null)
            backPanelImage.rectTransform.anchoredPosition = backPanelOpenPosition;
        if (informationPanel != null)
        {
            informationPanel.anchoredPosition = informationPanelOpenPosition + informationPanelStartOffset;
            informationPanel.gameObject.SetActive(false);
        }

        if (slideBackButton != null)
        {
            if (slideBackButtonRect != null)
                slideBackButtonRect.anchoredPosition = GetSlideBackButtonPosition();
            if (slideBackButtonGroup != null)
            {
                slideBackButtonGroup.alpha = 1f;
                slideBackButtonGroup.interactable = false;
                slideBackButtonGroup.blocksRaycasts = false;
            }

            slideBackButton.gameObject.SetActive(false);
        }

        RestoreInformationButtonInteractability();
        informationViewOpen = false;
    }

    private void RestoreInformationButtonInteractability()
    {
        for (int i = 0; i < informationEntries.Length && i < informationButtonInteractability.Count; i++)
        {
            if (informationEntries[i] != null && informationEntries[i].InformationButton != null)
                informationEntries[i].InformationButton.interactable = informationButtonInteractability[i];
        }
    }

    private void RestoreStateImmediately()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        ResetInformationView();
        if (elevatorCanvasGroup != null)
        {
            elevatorCanvasGroup.alpha = 0f;
            elevatorCanvasGroup.interactable = false;
            elevatorCanvasGroup.blocksRaycasts = false;
        }

        if (elevatorCanvas != null)
            elevatorCanvas.gameObject.SetActive(false);

        RestoreOtherCanvases();
        RestoreElevatorSorting();
        RestoreCursorState();
        RestorePlayerControls();
        isOpen = false;
        isClosing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
