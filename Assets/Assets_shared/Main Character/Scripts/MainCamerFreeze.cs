using UnityEngine;

public class MainCamerFreeze : MonoBehaviour
{
    // Bereichsgrenzen
    private float minX = -9.55f;
    private float maxX = 9.46f;
    private float minY = -4.45f;
    private float maxY = 4.73f;
    private float fixedZ = -7.31f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
