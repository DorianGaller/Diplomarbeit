using UnityEngine;

public class VentCanvasManager : MonoBehaviour
{
    public GameObject canvasToDisable;
    public GameObject canvasToEnable;

    public void SwitchCanvas()
    {
        if (canvasToDisable != null)
            canvasToDisable.SetActive(false);

        if (canvasToEnable != null)
            canvasToEnable.SetActive(true);
    }
}