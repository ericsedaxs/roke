using UnityEngine;

public class rocket : MonoBehaviour
{

    public float power = 100.0f;
    public Rigidbody rb;
    bool thrusterOn = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) { // GetKey, GetKeyDown, GetKeyUp
            thrusterOn = !thrusterOn;
            Debug.Log($"Thruster State: {thrusterOn}");
        }

        if (thrusterOn) {
            rb.AddForce(transform.up * power);
        }
        
    }
}
