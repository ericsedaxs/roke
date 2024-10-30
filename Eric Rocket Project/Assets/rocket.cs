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
        if (discreteActions[0] == 1) {
            thrusterOn = true;
        } else {
            thrusterOn = false;
        }

        if (thrusterOn) {
            rb.AddForce(transform.up * power);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;
        actions[0] = 0;
        if (Input.GetKeyDown(KeyCode.Return) && !thrusterOn) {
            actions[0] = 1;
        } else if (Input.GetKeyDown(KeyCode.Return) && thrusterOn) {
            actions[0] = 0;
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Return)) { // GetKey, GetKeyDown, GetKeyUp
    //         thrusterOn = !thrusterOn;
    //         Debug.Log($"Thruster State: {thrusterOn}");
    //     }

    //     if (thrusterOn) {
    //         rb.AddForce(transform.up * power);
    //     }
        
    // }
}
