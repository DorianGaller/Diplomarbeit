using UnityEngine;
using System.Collections.Generic;

public class VentManager : MonoBehaviour
{
    [Header("PLAYER")]
    public MonoBehaviour playerMovement;

    // -------------------- GAMEOBJECTS --------------------

    [Header("ON ENTER VENT - OBJECTS")]
    public List<GameObject> enableOnEnter = new List<GameObject>();
    public List<GameObject> disableOnEnter = new List<GameObject>();

    [Header("ON EXIT VENT - OBJECTS")]
    public List<GameObject> enableOnExit = new List<GameObject>();
    public List<GameObject> disableOnExit = new List<GameObject>();

    // -------------------- 2D COLLIDERS (SPRITES!) --------------------

    [Header("ON ENTER VENT - 2D COLLIDERS")]
    public List<Collider2D> enable2DCollidersOnEnter = new List<Collider2D>();
    public List<Collider2D> disable2DCollidersOnEnter = new List<Collider2D>();

    [Header("ON EXIT VENT - 2D COLLIDERS")]
    public List<Collider2D> enable2DCollidersOnExit = new List<Collider2D>();
    public List<Collider2D> disable2DCollidersOnExit = new List<Collider2D>();

    // -------------------- STATE --------------------

    private bool isInVent = false;
    public bool IsInVent => isInVent;

    // -------------------- MAIN --------------------

    public void ToggleVent()
    {
        // Sicherheit: Movement immer aktiv
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (!isInVent)
            EnterVent();
        else
            ExitVent();

        isInVent = !isInVent;
    }

    private void EnterVent()
    {
        ToggleObjects(enableOnEnter, disableOnEnter);
        Toggle2DColliders(enable2DCollidersOnEnter, disable2DCollidersOnEnter);

        if (playerMovement != null)
            playerMovement.enabled = true;

        ClearUIFocus();
    }

    private void ExitVent()
    {
        ToggleObjects(enableOnExit, disableOnExit);
        Toggle2DColliders(enable2DCollidersOnExit, disable2DCollidersOnExit);

        if (playerMovement != null)
            playerMovement.enabled = true;

        ClearUIFocus();
    }

    // -------------------- HELPERS --------------------

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


    private void Toggle2DColliders(List<Collider2D> enableList, List<Collider2D> disableList)
    {
        foreach (Collider2D col in enableList)
        {
            if (col != null)
                col.enabled = true;
        }

        foreach (Collider2D col in disableList)
        {
            if (col != null)
                col.enabled = false;
        }
    }

    private void ClearUIFocus()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
}