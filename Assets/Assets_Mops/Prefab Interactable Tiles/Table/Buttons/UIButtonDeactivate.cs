using UnityEngine;

public class UIButtonDeactivate : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    public void DeactivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Kein Target Object zugewiesen!");
        }
    }
}
