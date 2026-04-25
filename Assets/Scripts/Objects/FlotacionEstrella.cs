using UnityEngine;

public class FlotacionEstrella : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float amplitude = 0.5f;
    public float frequency = 1f;
    public float rotationSpeed = 100f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * frequency) * amplitude;
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

    }
}
