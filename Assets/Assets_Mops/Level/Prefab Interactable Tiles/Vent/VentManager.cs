using UnityEngine;
using System.Collections.Generic;

public class VentManager : MonoBehaviour
{
    [Header("ON ENTER VENT")]
    public List<GameObject> enableOnEnter = new List<GameObject>();
    public List<GameObject> disableOnEnter = new List<GameObject>();

    [Header("ON EXIT VENT")]
    public List<GameObject> enableOnExit = new List<GameObject>();
    public List<GameObject> disableOnExit = new List<GameObject>();

    private bool isInVent = false;

    public bool IsInVent => isInVent;

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
        ToggleObjects(enableOnEnter, disableOnEnter);
        ClearUIFocus();
    }

    private void ExitVent()
    {
        ToggleObjects(enableOnExit, disableOnExit);
        ClearUIFocus();
    }

    private void ToggleObjects(List<GameObject> enableList, List<GameObject> disableList)
    {
        foreach (GameObject obj in enableList)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        foreach (GameObject obj in disableList)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    private void ClearUIFocus()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current
                .SetSelectedGameObject(null);
    }
}