using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class rocket : Agent
{

    public float power = 100.0f;
    public Rigidbody rb;
    bool thrusterOn = false;
    public GameObject platform;
    public Material successMaterial;
    public Material failMaterial;

    private Vector3 previousPosition;
    public Vector3 currentVelocity { get; private set; }

    public GameObject mainThrusterParticles;
    public GameObject northThrusterParticles;
    public GameObject eastThrusterParticles;
    public GameObject southThrusterParticles;
    public GameObject westThrusterParticles;
    public TMP_Dropdown rocketDropdown;

    public GameObject falcon9_1;
    public GameObject falcon9_2;
    public GameObject falcon9_3;
    public GameObject saturnV;
    public GameObject falconHeavy;

    public TMP_InputField powerInput;
    public TMP_InputField massInput;

    float lastY = 1000f;
    float targetSpeed = -15.0f;


    private float lastXFromPlatform = 0;
    private float lastZFromPlatform = 0;
    private float lastXRotation = 0;
    private float lastZRotation = 0;

    public override void OnEpisodeBegin()
    {
        if (powerInput != null && massInput != null) {
            powerInput.text = power.ToString();
            massInput.text = rb.mass.ToString();
        }
        

        lastY = 1000f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(Random.Range(-200f, 200f), Random.Range(500f, 2000f), Random.Range(-200f, 200f));
        // TODO: set the lastXFromPlatform and lastZFromPlatform to the current distance the platform
        transform.localRotation = Quaternion.identity;
        previousPosition = transform.localPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); // 3
        sensor.AddObservation(rb.linearVelocity); // 3

        // rotation
        sensor.AddObservation(transform.rotation); // 4

        // drag
        sensor.AddObservation(rb.linearDamping); // 1

        // angular drag
        sensor.AddObservation(rb.angularDamping); // 1

        // mass
        sensor.AddObservation(rb.mass); // 1

        // gravity
        sensor.AddObservation(Physics.gravity); // 3

        // goal location
        sensor.AddObservation(platform.transform.localPosition); // 3

        // target speed
        sensor.AddObservation(targetSpeed); // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;
        Debug.Log($"Discrete Actions: {discreteActions[0]} {discreteActions[1]} {discreteActions[2]} {discreteActions[3]} {discreteActions[4]}");
        if (discreteActions[0] == 1) {
            thrusterOn = true;
            mainThrusterParticles.SetActive(true);
        } else {
            thrusterOn = false;
            mainThrusterParticles.SetActive(false);
        }

        if (thrusterOn) {
            rb.AddForce(transform.up * power);
        }

        if (discreteActions[1] == 1) {
            // Debug.Log("North Thruster On");
            // rotate the rocket north
            // rb.AddTorque(Vector3.right * 0.1f);
            // transform.Rotate(Vector3.right * 0.1f);

            // alternative: add force pushing rocket south
            rb.AddForce(transform.forward * power * 0.1f);
            northThrusterParticles.SetActive(true);
        } else {
            northThrusterParticles.SetActive(false);
        }

        if (discreteActions[2] == 1) {
            // Debug.Log("East Thruster On");
            // rotate the rocket east
            // rb.AddTorque(Vector3.forward * 0.1f);
            // transform.Rotate(Vector3.forward * 0.1f);
            // alternative: add force pushing rocket east
            rb.AddForce(-transform.right * power * 0.1f);
            eastThrusterParticles.SetActive(true);
        } else {
            eastThrusterParticles.SetActive(false);
        }

        if (discreteActions[3] == 1) {
            // Debug.Log("South Thruster On");
            // rotate the rocket south
            // rb.AddTorque(Vector3.right * -0.1f);
            // transform.Rotate(Vector3.right * -0.1f);
            // alternative: add force pushing rocket north
            rb.AddForce(-transform.forward * power * 0.1f);
            southThrusterParticles.SetActive(true);
        } else {
            southThrusterParticles.SetActive(false);
        }

        if (discreteActions[4] == 1) {
            // Debug.Log("West Thruster On");
            // rotate the rocket west
            // rb.AddTorque(Vector3.forward * -0.1f);
            // transform.Rotate(Vector3.forward * -0.1f);
            // alternative: add force pushing rocket west
            rb.AddForce(transform.right * power * 0.1f);
            westThrusterParticles.SetActive(true);
        } else {
            westThrusterParticles.SetActive(false);
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

            if (currentVelocity.y >= targetSpeed) {
                SetReward(10.0f);
                Debug.Log("Landed with a velocity of: " + currentVelocity.y + " m/s");
                platform.GetComponent<MeshRenderer>().material = successMaterial;
                EndEpisode();
            } else {
                AddReward(-10.0f);
                Debug.Log("Crashed with a velocity of: " + currentVelocity.y + " m/s");
                platform.GetComponent<MeshRenderer>().material = failMaterial;
                EndEpisode();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {

        // keep the rocket upright
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (powerInput != null && massInput != null) {
            power = float.Parse(powerInput.text);
            rb.mass = float.Parse(massInput.text);
        }

        currentVelocity = (transform.localPosition - previousPosition) / Time.deltaTime;
        // Debug.Log($"{transform.localPosition} - {previousPosition} / {Time.deltaTime} = {currentVelocity}");
        previousPosition = transform.localPosition;

        // check if the rocket is below the platform
        if (transform.localPosition.y < (platform.transform.localPosition.y - 10)) {
            Debug.Log("Failed because of going below the platform");
            AddReward(-15.0f);
            platform.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 1000f;
            EndEpisode();
        }

        // ignore in heuristic mode
        // bool isHeuristic = Academy.Instance.IsCommunicatorOn;
        if (transform.localPosition.y > (lastY)) {
            AddReward(-0.01f);
            // platform.GetComponent<MeshRenderer>().material = failMaterial;
            // lastY = 1000f;
            // EndEpisode();
        } else if (transform.localPosition.y < (lastY)) {
            // AddReward(0.01f);
        }

        if (transform.localPosition.y > 2000f) {
            Debug.Log("Failed because of going too high");
            SetReward(-20.0f);
            platform.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 1000f;
            EndEpisode();
        }


        // check horizontal position of the rocket to platform
        float x_distance = Mathf.Abs(transform.localPosition.x - platform.transform.localPosition.x);
        float z_distance = Mathf.Abs(transform.localPosition.z - platform.transform.localPosition.z);

        if ((x_distance > 20 || z_distance > 20) && (lastXFromPlatform < x_distance || lastZFromPlatform < z_distance)) {
            // add some punishment because the rocket is wandering away from the platform
            AddReward(-0.01f);
        }

        if (x_distance > 400 || z_distance > 400) {
            Debug.Log("Failed because of going too far away from the platform");
            AddReward(-15.0f);
            platform.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 1000f;
            EndEpisode();
        }


        // check the rotation if in the safe are to make it straight
        if (x_distance < 20 && z_distance < 20 && (transform.localRotation.x > 1 || transform.localRotation.z > 1)) {
            if (lastXRotation < transform.localRotation.x || lastZRotation < transform.localRotation.z) {
                AddReward(-0.01f);
            }
        }


        lastY = transform.localPosition.y;
        lastXFromPlatform = x_distance;
        lastZFromPlatform = z_distance;
        lastXRotation = transform.localRotation.x;
        lastZRotation = transform.localRotation.z;
    }

    public void changeRocket() {
        string chosenRocket = rocketDropdown.options[rocketDropdown.value].text;
        Debug.Log("Changing Rocket to: " + chosenRocket);

        if (chosenRocket == "Falcon 9") {
            falcon9_1.SetActive(true);
            falcon9_2.SetActive(true);
            falcon9_3.SetActive(true);
            saturnV.SetActive(false);
            falconHeavy.SetActive(false);
        } else if (chosenRocket == "Saturn V") {
            falcon9_1.SetActive(false);
            falcon9_2.SetActive(false);
            falcon9_3.SetActive(false);
            saturnV.SetActive(true);
            falconHeavy.SetActive(false);
        } else if (chosenRocket == "Falcon Heavy") {
            falcon9_1.SetActive(false);
            falcon9_2.SetActive(false);
            falcon9_3.SetActive(false);
            saturnV.SetActive(false);
            falconHeavy.SetActive(true);
        }
    }
}

// https://github.com/gzrjzcx/ML-agents/blob/master/docs/Training-Imitation-Learning.md