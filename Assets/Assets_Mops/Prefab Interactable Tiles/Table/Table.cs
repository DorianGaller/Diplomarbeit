using UnityEngine;

public class Table : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject tableUI;

    public void Interact(GameObject player)
    {
        tableUI.SetActive(true);
    }
}
