using UnityEngine;

public class Table : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject tableUI;
    [SerializeField] private float closeRange = 3f;   // NEU

    private Transform player;   // NEU

    private void Start()   // NEU
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()   // NEU
    {
        if (player == null || tableUI == null || !tableUI.activeSelf) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > closeRange)
            tableUI.SetActive(false);
    }

    public void Interact(GameObject player)
    {
        tableUI.SetActive(true);
    }
}