using UnityEngine;

public class PCTerminalUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject terminalRoot;

    [Header("Screens")]
    public GameObject desktopScreen;
    public GameObject[] appPanels;

    [Header("Optional")]
    public MonoBehaviour playerMovementToDisable;

    private bool isOpen;

    void Start() => Close();

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        isOpen = true;
        terminalRoot.SetActive(true);
        ShowDesktop();
        if (playerMovementToDisable != null) playerMovementToDisable.enabled = false;
    }

    public void Close()
    {
        isOpen = false;
        terminalRoot.SetActive(false);
        if (playerMovementToDisable != null) playerMovementToDisable.enabled = true;
    }

    public void ShowDesktop()
    {
        CloseAllApps();
        desktopScreen.SetActive(true);
    }

    public void OpenApp(int index)
    {
        desktopScreen.SetActive(false);
        CloseAllApps();
        if (index >= 0 && index < appPanels.Length)
            appPanels[index].SetActive(true);
    }

    private void CloseAllApps()
    {
        foreach (var panel in appPanels)
            if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            bool anyAppOpen = false;
            foreach (var p in appPanels)
                if (p != null && p.activeSelf) anyAppOpen = true;

            if (anyAppOpen) ShowDesktop();
            else Close();
        }
    }
}