using UnityEngine;
using UnityEngine.InputSystem;

public class InteractKeys : MonoBehaviour
{
    private InputAction gunshotAction;
    private InputAction interactAction;

    public GameObject gunshotPrefab;
    public Transform shootPoint;

    void Awake()
    {
        gunshotAction = new InputAction(
            name: "Gunshot",
            type: InputActionType.Button,
            binding: "<Mouse>/leftButton"
        );

        interactAction = new InputAction(
            name: "Interact",
            type: InputActionType.Button,
            binding: "<Keyboard>/e"
        );
    }

    void OnEnable()
    {
        gunshotAction.Enable();
        interactAction.Enable();
    }

    void OnDisable()
    {
        gunshotAction.Disable();
        interactAction.Disable();
    }

    void Update()
    {
        if (gunshotAction.WasPressedThisFrame())
        {
            OnGunshot();
        }

        if (interactAction.WasPressedThisFrame())
        {
            OnInteract();
        }
    }

    private void OnGunshot()
    {
        if (!gunshotPrefab || !shootPoint) return;

        
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        
        Vector2 shootDir = (mouseWorldPos - shootPoint.position).normalized;

        
        GameObject shot = Instantiate(
            gunshotPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        shot.GetComponent<GunshotEffect>()?.Init(shootDir);
    }

    private void OnInteract()
    {
        Debug.Log("Interact!");
    }
}
