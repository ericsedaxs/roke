using UnityEngine;

public class rotationTest : MonoBehaviour
{
    public GameObject target;
    public float x_offset = 0;
    public float y_offset = 0;
    public float z_offset = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // point the top of the object to towards the target gradually
        Vector3 targetPosition = target.transform.position;
        Vector3 direction = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        targetRotation *= Quaternion.Euler(x_offset, y_offset, z_offset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2);

    }
}
