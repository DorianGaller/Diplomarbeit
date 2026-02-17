using UnityEngine;
using UnityEngine.UI;

public class DesktopAppButton : MonoBehaviour
{
    [Header("App Settings")]
    public string appName = "App";

    private PCTerminalUI terminalUI;
    private Button button;

    void Start()
    {
        terminalUI = FindObjectOfType<PCTerminalUI>();
        button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (terminalUI != null)
            terminalUI.OpenApp(appName);
    }
}