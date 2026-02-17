using UnityEngine;

public class VentManager : MonoBehaviour
{
    [Header("Canvas On Enter Vent")]
    public GameObject canvasToEnableOnEnter;
    public GameObject canvasToDisableOnEnter;

    [Header("Canvas On Exit Vent")]
    public GameObject canvasToEnableOnExit;
    public GameObject canvasToDisableOnExit;

    private bool isInVent = false;

    public void ToggleVent()
    {
        if (!isInVent)
        {
            EnterVent();
        }
        else
        {
            ExitVent();
        }

        isInVent = !isInVent;
    }

    private void EnterVent()
    {
        if (canvasToDisableOnEnter != null)
            canvasToDisableOnEnter.SetActive(false);

        if (canvasToEnableOnEnter != null)
            canvasToEnableOnEnter.SetActive(true);

        ClearUIFocus();
    }

    private void ExitVent()
    {
        if (canvasToDisableOnExit != null)
            canvasToDisableOnExit.SetActive(false);

        if (canvasToEnableOnExit != null)
            canvasToEnableOnExit.SetActive(true);

        ClearUIFocus();
    }

    private void ClearUIFocus()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current
                .SetSelectedGameObject(null);
    }
}
