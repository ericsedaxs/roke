using UnityEngine;

public class TitleRocket : MonoBehaviour
{
    public float power = 100.0f;
    public Rigidbody rb;

    public Transform target;

    public Vector3 startPosition;
    public Vector3 endPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set timer for 6 seconds and call turn around
        // Invoke("TurnAround", 6);
        startPosition = new Vector3(-60, 20, -30);
        endPosition = new Vector3(157, 0, 445);
    }

    // Update is called once per frame
    void Update()
    {
        // look at target, but from the top of the rocket instead of the side of the rocket
        
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        


        rb.AddForce(transform.forward * power);
    }

    public void BeginMovement() {
        // enable rb constraints
        rb.constraints = RigidbodyConstraints.None;
        
        Invoke("TurnAround", 6);
    }

    void TurnAround()
    {
        Debug.Log("Turning around");
        
        target.position = endPosition;
    }
}
