using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PCTerminalUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject terminalPanel;   // Das Haupt-Panel
    public GameObject desktopPanel;    // Der Desktop
    public GameObject appWindowPanel;  // App-Fenster (Platzhalter)

    [Header("App Window")]
    public TextMeshProUGUI appTitleText;
    public TextMeshProUGUI appContentText;
    public Button closeAppButton;

    [Header("Taskbar")]
    public TextMeshProUGUI clockText;

    [Header("Player Control")]
    public MonoBehaviour playerController; // Dein Player Movement Script
    public bool lockCursorOnOpen = true;

    public bool IsOpen { get; private set; } = false;

    void Start()
    {
        // Panel am Start schließen
        if (terminalPanel != null) terminalPanel.SetActive(false);
        if (appWindowPanel != null) appWindowPanel.SetActive(false);

        if (closeAppButton != null)
            closeAppButton.onClick.AddListener(CloseApp);
    }

    void Update()
    {
        // Uhr aktualisieren
        if (clockText != null && IsOpen)
            clockText.text = DateTime.Now.ToString("HH:mm");

        // ESC zum Schließen
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseTerminal();
    }

    public void OpenTerminal()
    {
        IsOpen = true;
        terminalPanel.SetActive(true);
        desktopPanel.SetActive(true);
        if (appWindowPanel != null) appWindowPanel.SetActive(false);

        // Spieler einfrieren & Cursor freigeben
        if (playerController != null)
            playerController.enabled = false;

        if (lockCursorOnOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CloseTerminal()
    {
        IsOpen = false;
        terminalPanel.SetActive(false);

        // Spieler wieder aktivieren
        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Wird von App-Buttons aufgerufen
    public void OpenApp(string appName)
    {
        if (appWindowPanel == null) return;

        appWindowPanel.SetActive(true);

        if (appTitleText != null)
            appTitleText.text = appName;

        if (appContentText != null)
            appContentText.text = GetPlaceholderContent(appName);
    }

    public void CloseApp()
    {
        if (appWindowPanel != null)
            appWindowPanel.SetActive(false);
    }

    private string GetPlaceholderContent(string appName)
    {
        switch (appName)
        {
            case "E-Mail":
                return "Keine neuen Nachrichten.\n\n[Posteingang leer]";
            case "Terminal":
                return "> System bereit\n> Benutzer: ADMIN\n> Letzter Login: " + DateTime.Now.ToString("dd.MM.yyyy");
            case "Browser":
                return "Kein Internetzugang.\nNetzwerk getrennt.";
            case "Dateien":
                return "C:/Dokumente\nC:/Bilder\nC:/System\n\n[3 Ordner gefunden]";
            default:
                return $"[{appName}]\n\nDiese Anwendung ist ein Platzhalter.";
        }
    }
}