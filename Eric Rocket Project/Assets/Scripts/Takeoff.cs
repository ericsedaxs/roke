using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class Takeoff : Agent
{

    public float power = 100.0f;
    public Rigidbody rb;
    bool thrusterOn = false;
    public GameObject platform;
    public GameObject indicator;
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
    float targetSpeed = 25.0f;

    float highestHeight = 0;

    private float lastXFromPlatform = 0;
    private float lastZFromPlatform = 0;
    private float lastXRotation = 0;
    private float lastZRotation = 0;
    private float lastDistance = 1000f;
    private float closestDistance = 1000f;

    // start time
    private float startTime;

    public override void OnEpisodeBegin()
    {
        startTime = Time.time;
        highestHeight = transform.localPosition.y;

        if (powerInput != null && massInput != null) {
            powerInput.text = power.ToString();
            massInput.text = rb.mass.ToString();
        }
        

        lastY = 0.5f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0.5f, 0);
        platform.transform.localPosition = new Vector3(Random.Range(-200f, 200f), Random.Range(500f, 2000f), Random.Range(-200f, 200f));
        // TODO: set the lastXFromPlatform and lastZFromPlatform to the current distance the platform
        transform.localRotation = Quaternion.identity;
        previousPosition = transform.localPosition;

        lastDistance = Vector3.Distance(transform.localPosition, platform.transform.localPosition);
        closestDistance = lastDistance;
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
        // Debug.Log($"Discrete Actions: {discreteActions[0]} {discreteActions[1]} {discreteActions[2]} {discreteActions[3]} {discreteActions[4]}");
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

            // overall speed
            float overallSpeed = currentVelocity.magnitude;

            if (overallSpeed < targetSpeed) {
                // calculate time bonus
                float timeBonus = (1.0f - ((Time.time - startTime) / 60)) * 5;

                AddReward(10.0f + timeBonus);
                Debug.Log("Reached goal at: " + overallSpeed + " m/s" + " Total Time: " + (Time.time - startTime) + " seconds" + " Time Bonus: " + timeBonus);
                // Debug.Log($"Success Overall Speed: {overallSpeed} m/s");
                indicator.GetComponent<MeshRenderer>().material = successMaterial;
                Stats.Instance.successCount++;
                EndEpisode();
            } else {
                AddReward(-10.0f);
                Debug.Log("Crashed with at: " + overallSpeed + " m/s" + " Total Time: " + (Time.time - startTime) + " seconds");
                // Debug.Log($"Fail Overall Speed: {overallSpeed} m/s");
                indicator.GetComponent<MeshRenderer>().material = failMaterial;
                Stats.Instance.crashCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }
        } 
        // else if (collision.gameObject.CompareTag("Ground") && highestHeight > 10) {
        //     Debug.Log("Crashed because of hitting the ground");
        //     AddReward(-15.0f);
        //     indicator.GetComponent<MeshRenderer>().material = failMaterial;
        //     highestHeight = 0;
        //     lastY = 0f;
        //     EndEpisode();
        // }
    }

    // Update is called once per frame
    void FixedUpdate() {

        // check if the rocket took too long and fail if past 60 seconds
        // Debug.Log($"Time: {Time.time - startTime}");
        if (Time.time - startTime > 60) {
            Debug.Log("Failed because of taking too long");
            AddReward(-20.0f);
            indicator.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 0f;
            Stats.Instance.failureCount++;
            EndEpisode();
        }

        // TODO: add check for if the rocket is slowing down when close to goal
        float distanceToGoal = Vector3.Distance(transform.localPosition, platform.transform.localPosition);
        if (distanceToGoal < 100f) {
            // check if the rocket is slowing down
            float lastVelocity = currentVelocity.magnitude;
            float currentVelocityMagnitude = ((transform.localPosition - previousPosition) / Time.deltaTime).magnitude;

            if (currentVelocityMagnitude < lastVelocity) {
                // add a reward for slowing down
                AddReward(0.01f);
            } else {
                // add a punishment for not slowing down
                AddReward(-0.01f);
            }

        }

        // if the rocket is farther away than last frame, add a punishment
        if (distanceToGoal > lastDistance) {
            AddReward(-0.01f);
        }
        // outright fail if the current distance is 10m or more than the closest distance
        if (distanceToGoal > closestDistance + 30f) {
            Debug.Log("Failed because of going too far away from the platform");
            AddReward(-15.0f);
            indicator.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 0f;
            Stats.Instance.failureCount++;
            EndEpisode();
        }

        lastDistance = distanceToGoal;

        if (closestDistance < distanceToGoal) {
            closestDistance = distanceToGoal;
        }


        if (transform.localPosition.y > 5f){
            transform.rotation = Quaternion.LookRotation(platform.transform.position - transform.position, Vector3.up);
            // add an offset of +90, 0 ,0 to make the rocket upright
            transform.Rotate(90, 0, 0);
        }
        

        if (powerInput != null && massInput != null) {
            power = float.Parse(powerInput.text);
            rb.mass = float.Parse(massInput.text);
        }

        currentVelocity = (transform.localPosition - previousPosition) / Time.deltaTime;
        // Debug.Log($"{transform.localPosition} - {previousPosition} / {Time.deltaTime} = {currentVelocity}");
        previousPosition = transform.localPosition;

        // check if the rocket is above the goal
        if (transform.localPosition.y > (platform.transform.localPosition.y + 10)) {
            Debug.Log("Failed because of going above the target");
            AddReward(-15.0f);
            indicator.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 0f;
            Stats.Instance.failureCount++;
            EndEpisode();
        }

        // ignore in heuristic mode
        // bool isHeuristic = Academy.Instance.IsCommunicatorOn;
        if (transform.localPosition.y < (lastY)) {
            AddReward(-0.01f);
            // platform.GetComponent<MeshRenderer>().material = failMaterial;
            // lastY = 1000f;
            // EndEpisode();
        } else if (transform.localPosition.y > (lastY)) {
            AddReward(0.005f);
        }

        if (transform.localPosition.y < 0f) {
            Debug.Log("Failed because of going too low");
            SetReward(-30.0f);
            indicator.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 0f;
            Stats.Instance.failureCount++;
            EndEpisode();
        }


        // check horizontal position of the rocket to platform
        float x_distance = Mathf.Abs(transform.localPosition.x - platform.transform.localPosition.x);
        float z_distance = Mathf.Abs(transform.localPosition.z - platform.transform.localPosition.z);

        if ((x_distance > 20 || z_distance > 20) && (lastXFromPlatform < x_distance || lastZFromPlatform < z_distance)) {
            // add some punishment because the rocket is wandering away from the platform
            AddReward(-0.01f);
        }

        if (x_distance > 200 || z_distance > 200) {
            Debug.Log("Failed because of going too far away from the platform");
            AddReward(-15.0f);
            platform.GetComponent<MeshRenderer>().material = failMaterial;
            lastY = 1000f;
            Stats.Instance.failureCount++;
            EndEpisode();
        }


        // check if the rocket fell too much
        // if (transform.localPosition.y > 2 && transform.localPosition.y < (highestHeight - 5)) {
        //     Debug.Log("Failed because of falling too much");
        //     AddReward(-15.0f);
        //     indicator.GetComponent<MeshRenderer>().material = failMaterial;
        //     highestHeight = 0;
        //     lastY = 0f;
        //     EndEpisode();
        // }


        // check the rotation if in the safe are to make it straight
        // if (x_distance < 20 && z_distance < 20 && (transform.localRotation.x > 1 || transform.localRotation.z > 1)) {
        //     if (lastXRotation < transform.localRotation.x || lastZRotation < transform.localRotation.z) {
        //         AddReward(-0.01f);
        //     }
        // }


        lastY = transform.localPosition.y;

        if (lastY > highestHeight) {
            highestHeight = lastY;
        }

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