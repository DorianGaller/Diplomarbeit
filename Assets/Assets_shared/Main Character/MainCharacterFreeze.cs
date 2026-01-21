using UnityEngine;

public class MainCharacterFreeze : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float minX = -10.85f;
    private float maxX = 10.85f;
    private float minY = -5.18f;
    private float maxY = 4.68f;
    private float fixedZ = -2.69f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize any necessary components or variables here
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = fixedZ;

        transform.position = pos;
    }
}