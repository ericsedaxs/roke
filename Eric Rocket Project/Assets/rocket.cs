using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class rocket : Agent
{

    public float power = 100.0f;
    public Rigidbody rb;
    bool thrusterOn = false;
    public GameObject platform;
    public Material successMaterial;
    public Material failMaterial;

    float lastY = 1000f;

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 500f, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity);

        // rotation
        sensor.AddObservation(transform.rotation);

        // drag
        sensor.AddObservation(rb.linearDamping);

        // angular drag
        sensor.AddObservation(rb.angularDamping);

        // mass
        sensor.AddObservation(rb.mass);

        // gravity
        sensor.AddObservation(Physics.gravity);

        // goal location
        sensor.AddObservation(platform.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        // Debug.Log(discreteActions[0]);
        if (discreteActions[0] == 1) {
            thrusterOn = true;
        } else {
            thrusterOn = false;
        }

        if (thrusterOn) {
            rb.AddForce(transform.up * power);
        }

        if (discreteActions[1] == 1) {
            Debug.Log("North Thruster On");
            // rotate the rocket north
            transform.Rotate(Vector3.right * 0.1f);
        }
        if (discreteActions[2] == 1) {
            Debug.Log("East Thruster On");
            // rotate the rocket east
            transform.Rotate(Vector3.forward * 0.1f);
        }
        if (discreteActions[3] == 1) {
            Debug.Log("South Thruster On");
            // rotate the rocket south
            transform.Rotate(Vector3.right * -0.1f);
        }
        if (discreteActions[4] == 1) {
            Debug.Log("West Thruster On");
            // rotate the rocket west
            transform.Rotate(Vector3.forward * -0.1f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        actions[0] = 0;
        actions[1] = 0;
        actions[2] = 0;
        actions[3] = 0;
        actions[4] = 0;
        // if (Input.GetKeyDown(KeyCode.Return) && !thrusterOn) {
        //     Debug.Log("Thruster On");
        //     actions[0] = 1;
        // } else if (Input.GetKeyDown(KeyCode.Return) && thrusterOn) {
        //     Debug.Log("Thruster Off");
        //     actions[0] = 0;
        // }
        if (Input.GetKey(KeyCode.Return)) {
            actions[0] = 1;
        }

        if (Input.GetKey(KeyCode.W)) {
            actions[1] = 1;
        }

        if (Input.GetKey(KeyCode.A)) {
            actions[2] = 1;
        }

        if (Input.GetKey(KeyCode.S)) {
            actions[3] = 1;
        }

        if (Input.GetKey(KeyCode.D)) {
            actions[4] = 1;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Goal")) {
            // check vertical speed
            Debug.Log($"This was the speed: {rb.linearVelocity.magnitude}");
            Debug.Log($"This was the y: {rb.linearVelocity.y}");
            if (rb.linearVelocity.magnitude <= 0 && rb.linearVelocity.magnitude >= -5) {
                SetReward(1.0f);
                platform.GetComponent<MeshRenderer>().material = successMaterial;
                EndEpisode();
            } else {
                SetReward(-1.0f);
                platform.GetComponent<MeshRenderer>().material = failMaterial;
                EndEpisode();
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if (transform.localPosition.y < platform.transform.localPosition.y) {
            SetReward(-1.0f);
            platform.GetComponent<MeshRenderer>().material = failMaterial;
            EndEpisode();
        }

        if (transform.localPosition.y > (lastY)) {
            SetReward(-1.0f);
            platform.GetComponent<MeshRenderer>().material = failMaterial;
            EndEpisode();
        }

        lastY = transform.localPosition.y;
    }
}
