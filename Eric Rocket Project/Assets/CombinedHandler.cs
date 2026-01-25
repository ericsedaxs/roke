using UnityEngine;

using Unity.MLAgents.Policies;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class CombinedHandler : Agent
{
    // Takeoff onnx model file
    public Unity.InferenceEngine.ModelAsset takeoffModel;
    // Landing onnx model file
    public Unity.InferenceEngine.ModelAsset landingModel;

    // Behavior parameters
    public BehaviorParameters behaviorParameters;
    public string currentMode = "takeoff";


    public float power = 100.0f;
    public Rigidbody rb;
    bool thrusterOn = false;
    public GameObject target;
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
    public TextMeshProUGUI controlLable;

    float lastY = 1000f;
    float targetSpeed = 50.0f;

    float highestHeight = 0;

    private float lastXFromPlatform = 0;
    private float lastZFromPlatform = 0;
    private float lastXRotation = 0;
    private float lastZRotation = 0;
    private float lastDistance = 1000f;
    private float closestDistance = 1000f;
    private bool control = false;

    // start time
    private float startTime;

    public void Controller() {

        control = !control;

        if (control)
        {
            behaviorParameters.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
            controlLable.text = "Human Controller";
        }
        else
        {
            behaviorParameters.BehaviorType = Unity.MLAgents.Policies.BehaviorType.Default;
            controlLable.text = "AI Controller";
        }
    }

    void Start() {
        // set initial model to takeoff model
        behaviorParameters.Model = takeoffModel;
    }

    public override void OnEpisodeBegin()
    {
        if (currentMode == "landing") {
            SwitchModes();
        }

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
        target.transform.localPosition = new Vector3(UnityEngine.Random.Range(-200f, 200f), UnityEngine.Random.Range(500f, 2000f), UnityEngine.Random.Range(-200f, 200f));
        // TODO: set the lastXFromPlatform and lastZFromPlatform to the current distance the platform
        transform.localRotation = Quaternion.identity;
        previousPosition = transform.localPosition;

        lastDistance = Vector3.Distance(transform.localPosition, target.transform.localPosition);
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
        sensor.AddObservation(target.transform.localPosition); // 3

        // target speed
        sensor.AddObservation(targetSpeed); // 1
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (currentMode == "landing") {
    
        var discreteActions = actions.DiscreteActions;
        // Debug.Log($"Discrete Actions: {discreteActions[0]} {discreteActions[1]} {discreteActions[2]} {discreteActions[3]} {discreteActions[4]}");

        if (rb.linearVelocity.y >= 0 && discreteActions[0] == 1) {
            AddReward(-0.1f);
            discreteActions[0] = 0; // shut off the main thruster if we are going up
        }

        float yDistanceToPlatform = Mathf.Abs(transform.localPosition.y - target.transform.localPosition.y);

        if (yDistanceToPlatform < 400f && rb.linearVelocity.y < -40f && discreteActions[0] == 0) {
            AddReward(-0.1f);
            discreteActions[0] = 1; // turn on the main thruster if we are going down too fast
        }

        if (yDistanceToPlatform < 200f && rb.linearVelocity.y < -15f && discreteActions[0] == 0) {
            AddReward(-0.1f);
            discreteActions[0] = 1; // turn on the main thruster if we are going down too fast
        }

        if (discreteActions[0] == 1) {
            thrusterOn = true;
            // mainThrusterParticles.SetActive(true);
            mainThrusterParticles.GetComponent<ParticleSystem>().Play();
        } else {
            thrusterOn = false;
            // mainThrusterParticles.SetActive(false);
            mainThrusterParticles.GetComponent<ParticleSystem>().Stop();
        }

        if (thrusterOn) {
            rb.AddForce(transform.up * power);
        }

        // Check if valid thrusters
        // Step 1: check the horizontal position of the rocket to the platform (both x and z)
        float x_distance = Mathf.Abs(transform.localPosition.x - target.transform.localPosition.x);
        float x_difference = transform.localPosition.x - target.transform.localPosition.x;
        float z_distance = Mathf.Abs(transform.localPosition.z - target.transform.localPosition.z);
        float z_difference = transform.localPosition.z - target.transform.localPosition.z;

        // Debug.Log($"X Distance: {x_distance} Z Distance: {z_distance}");
        // Debug.Log($"X Difference: {x_difference} Z Difference: {z_difference}");

        // Step 2: check the rotation of the rocket about the y axis
        float y_rotation = transform.localRotation.y;

        // Step 3: check the velocity of the rocket (x and z)
        float x_velocity = rb.linearVelocity.x;
        float z_velocity = rb.linearVelocity.z;

        // Debug.Log($"X Velocity: {x_velocity} Z Velocity: {z_velocity}");

        // Debug.Log($"X Distance: {x_distance} Z Distance: {z_distance} X Velocity: {x_velocity} Z Velocity: {z_velocity}");

        // Step 4: determine which side thruster to turn on to push it towards the x and z of the platform
        if (x_distance > 5) {
            int correctThruster = -1;
            if (transform.localPosition.x > target.transform.localPosition.x) {
                // rocket is to the right of the platform
                // turn on the east thruster
                correctThruster = 2;
                discreteActions[2] = 1;

                // if we are getting close but too fast, turn on the west thruster and shut off the east thruster (use x_difference so we are direction aware)
                if ((x_difference < 150 && x_velocity < -10) || (x_difference < 20 && x_velocity < -0.5)) {
                    // Debug.Log("Getting close but too fast, turning on west thruster");
                    discreteActions[2] = 0;
                    discreteActions[4] = 1;
                    correctThruster = 4;
                }
            } else {
                // rocket is to the left of the platform
                // turn on the west thruster
                correctThruster = 4;
                discreteActions[4] = 1;

                // if we are getting close but too fast, turn on the east thruster
                if ((x_difference > -150 && x_velocity > 10) || (x_difference > -20 && x_velocity > 0.5)) {
                    // Debug.Log("Getting close but too fast, turning on east thruster");
                    discreteActions[4] = 0;
                    discreteActions[2] = 1;
                    correctThruster = 2;
                }
            }

            // if (correctThruster == 2 && discreteActions[2] == 1 && discreteActions[4] == 0) {
            //     AddReward(0.02f); // The correct thrusters are on/off
            // } else if (correctThruster == 4 && discreteActions[2] == 0 && discreteActions[4] == 1) {
            //     AddReward(0.02f); // The correct thrusters are on/off
            // } else {
            //     AddReward(-0.01f); // The correct thrusters are not on/off
            //     // Debug.Log("Incorrect Thruster On");
            // }
        }

        if (z_distance > 5) {
            int correctThruster = -1;
            if (transform.localPosition.z > target.transform.localPosition.z) {
                // rocket is to the north of the platform
                // turn on the south thruster
                discreteActions[3] = 1;
                correctThruster = 3;

                // if we are getting close but too fast, turn on the north thruster
                if ((z_difference < 150 && z_velocity < -10) || (z_difference < 20 && z_velocity < -0.5)) {
                    // Debug.Log("Getting close but too fast, turning on north thruster");
                    discreteActions[3] = 0;
                    discreteActions[1] = 1;
                    correctThruster = 1;
                }
                
            } else {
                // rocket is to the south of the platform
                // turn on the north thruster
                discreteActions[1] = 1;
                correctThruster = 1;

                // if we are getting close but too fast, turn on the south thruster
                if ((z_difference > -150 && z_velocity > 10) || (z_difference > -20 && z_velocity > 0.5)) {
                    // Debug.Log("Getting close but too fast, turning on south thruster");
                    discreteActions[1] = 0;
                    discreteActions[3] = 1;
                    correctThruster = 3;
                }
            }

            // if (correctThruster == 1 && discreteActions[1] == 1 && discreteActions[3] == 0) {
            //     AddReward(0.02f); // The correct thrusters are on/off
            // } else if (correctThruster == 3 && discreteActions[1] == 0 && discreteActions[3] == 1) {
            //     AddReward(0.02f); // The correct thrusters are on/off
            // } else {
            //     AddReward(-0.01f); // The correct thrusters are not on/off
            //     // Debug.Log("Incorrect Thruster On");
            // }
        }


        if (discreteActions[1] == 1) {
            // Debug.Log("North Thruster On");
            // rotate the rocket north
            // rb.AddTorque(Vector3.right * 0.1f);
            // transform.Rotate(Vector3.right * 0.1f);

            // alternative: add force pushing rocket south
            rb.AddForce(transform.forward * power * 0.1f);
            // northThrusterParticles.SetActive(true);
            northThrusterParticles.GetComponent<ParticleSystem>().Play();
        } else {
            // northThrusterParticles.SetActive(false);
            northThrusterParticles.GetComponent<ParticleSystem>().Stop();
        }

        if (discreteActions[2] == 1) {
            // Debug.Log("East Thruster On");
            // rotate the rocket east
            // rb.AddTorque(Vector3.forward * 0.1f);
            // transform.Rotate(Vector3.forward * 0.1f);
            // alternative: add force pushing rocket east
            rb.AddForce(-transform.right * power * 0.1f);
            // eastThrusterParticles.SetActive(true);
            eastThrusterParticles.GetComponent<ParticleSystem>().Play();
        } else {
            // eastThrusterParticles.SetActive(false);
            eastThrusterParticles.GetComponent<ParticleSystem>().Stop();
        }

        if (discreteActions[3] == 1) {
            // Debug.Log("South Thruster On");
            // rotate the rocket south
            // rb.AddTorque(Vector3.right * -0.1f);
            // transform.Rotate(Vector3.right * -0.1f);
            // alternative: add force pushing rocket north
            rb.AddForce(-transform.forward * power * 0.1f);
            // southThrusterParticles.SetActive(true);
            southThrusterParticles.GetComponent<ParticleSystem>().Play();
        } else {
            // southThrusterParticles.SetActive(false);
            southThrusterParticles.GetComponent<ParticleSystem>().Stop();
        }

        if (discreteActions[4] == 1) {
            // Debug.Log("West Thruster On");
            // rotate the rocket west
            // rb.AddTorque(Vector3.forward * -0.1f);
            // transform.Rotate(Vector3.forward * -0.1f);
            // alternative: add force pushing rocket west
            rb.AddForce(transform.right * power * 0.1f);
            // westThrusterParticles.SetActive(true);
            westThrusterParticles.GetComponent<ParticleSystem>().Play();
        } else {
            // westThrusterParticles.SetActive(false);
            westThrusterParticles.GetComponent<ParticleSystem>().Stop();
        }
        } else {
            var discreteActions = actions.DiscreteActions;
            // Debug.Log($"Discrete Actions: {discreteActions[0]} {discreteActions[1]} {discreteActions[2]} {discreteActions[3]} {discreteActions[4]}");
            if (discreteActions[0] == 1) {
                thrusterOn = true;
                // mainThrusterParticles.SetActive(true);
                mainThrusterParticles.GetComponent<ParticleSystem>().Play();
            } else {
                thrusterOn = false;
                // mainThrusterParticles.SetActive(false);
                mainThrusterParticles.GetComponent<ParticleSystem>().Stop();
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
                // northThrusterParticles.SetActive(true);
                northThrusterParticles.GetComponent<ParticleSystem>().Play();
            } else {
                // northThrusterParticles.SetActive(false);
                northThrusterParticles.GetComponent<ParticleSystem>().Stop();
            }

            if (discreteActions[2] == 1) {
                // Debug.Log("East Thruster On");
                // rotate the rocket east
                // rb.AddTorque(Vector3.forward * 0.1f);
                // transform.Rotate(Vector3.forward * 0.1f);
                // alternative: add force pushing rocket east
                rb.AddForce(-transform.right * power * 0.1f);
                // eastThrusterParticles.SetActive(true);
                eastThrusterParticles.GetComponent<ParticleSystem>().Play();
            } else {
                // eastThrusterParticles.SetActive(false);
                eastThrusterParticles.GetComponent<ParticleSystem>().Stop();
            }

            if (discreteActions[3] == 1) {
                // Debug.Log("South Thruster On");
                // rotate the rocket south
                // rb.AddTorque(Vector3.right * -0.1f);
                // transform.Rotate(Vector3.right * -0.1f);
                // alternative: add force pushing rocket north
                rb.AddForce(-transform.forward * power * 0.1f);
                // southThrusterParticles.SetActive(true);
                southThrusterParticles.GetComponent<ParticleSystem>().Play();
            } else {
                // southThrusterParticles.SetActive(false);
                southThrusterParticles.GetComponent<ParticleSystem>().Stop();
            }

            if (discreteActions[4] == 1) {
                // Debug.Log("West Thruster On");
                // rotate the rocket west
                // rb.AddTorque(Vector3.forward * -0.1f);
                // transform.Rotate(Vector3.forward * -0.1f);
                // alternative: add force pushing rocket west
                rb.AddForce(transform.right * power * 0.1f);
                // westThrusterParticles.SetActive(true);
                westThrusterParticles.GetComponent<ParticleSystem>().Play();
            } else {
                // westThrusterParticles.SetActive(false);
                westThrusterParticles.GetComponent<ParticleSystem>().Stop();
            }
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
        if (currentMode == "takeoff") {
            if (collision.gameObject.CompareTag("Goal")) {
                // check vertical speed

                // overall speed
                float overallSpeed = currentVelocity.magnitude;

                // === DEMO ===
                Debug.Log("Reached goal at: " + overallSpeed + " m/s" + " Total Time: " + (Time.time - startTime) + " seconds");
                // Debug.Log($"Success Overall Speed: {overallSpeed} m/s");
                target.GetComponent<MeshRenderer>().material = successMaterial;
                // Stats.Instance.successCount++;
                // EndEpisode();
                // Instead of ending the episode, change modes to landing
                SwitchModes();
                // ============

                // if (overallSpeed < targetSpeed) {
                //     // calculate time bonus
                //     float timeBonus = (1.0f - ((Time.time - startTime) / 60)) * 5;

                //     if (GetCumulativeReward() < 0) {
                //         AddReward(-1 * GetCumulativeReward());
                //     }

                //     AddReward(10.0f + timeBonus);
                //     Debug.Log("Reached goal at: " + overallSpeed + " m/s" + " Total Time: " + (Time.time - startTime) + " seconds" + " Time Bonus: " + timeBonus);
                //     // Debug.Log($"Success Overall Speed: {overallSpeed} m/s");
                //     indicator.GetComponent<MeshRenderer>().material = successMaterial;
                //     // Stats.Instance.successCount++;
                //     // EndEpisode();
                //     // Instead of ending the episode, change modes to landing
                //     SwitchModes();
                // } else {
                //     if (GetCumulativeReward() > 0) {
                //         AddReward(-1 * GetCumulativeReward());
                //     }
                //     AddReward(-10.0f - (overallSpeed - targetSpeed));
                //     Debug.Log("Crashed with at: " + overallSpeed + " m/s" + " Total Time: " + (Time.time - startTime) + " seconds");
                //     // Debug.Log($"Fail Overall Speed: {overallSpeed} m/s");
                //     indicator.GetComponent<MeshRenderer>().material = failMaterial;
                //     // Stats.Instance.crashCount++;
                //     // Stats.Instance.failureCount++;
                //     EndEpisode();
                // }
            } 
        } else if (currentMode == "landing") {
            if (collision.gameObject.CompareTag("Ground")) {
            // check vertical speed
            float currentTime = Time.time;
            float timeElapsed = currentTime - startTime;

            if (currentVelocity.y >= targetSpeed) {
                SetReward(10.0f);
                float timeBonus = (600 / timeElapsed) - 10;
                AddReward(timeBonus);
                Debug.Log("Landed with a velocity of: " + currentVelocity.y + " m/s in " + timeElapsed + " seconds");
                target.GetComponent<MeshRenderer>().material = successMaterial;
                Stats.Instance.successCount++;
                EndEpisode();
            } else {
                AddReward(-10.0f);
                Debug.Log("Crashed with a velocity of: " + currentVelocity.y + " m/s");
                target.GetComponent<MeshRenderer>().material = failMaterial;
                Stats.Instance.crashCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }
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
        // check horizontal position of the rocket to platform
        float x_distance = Mathf.Abs(transform.localPosition.x - target.transform.localPosition.x);
        float z_distance = Mathf.Abs(transform.localPosition.z - target.transform.localPosition.z);



        if (currentMode == "takeoff") {
            // check if the rocket took too long and fail if past 60 seconds
            // Debug.Log($"Time: {Time.time - startTime}");
            if (Time.time - startTime > 180) {
                Debug.Log("Failed because of taking too long");
                if (GetCumulativeReward() > 0) {
                    AddReward(-1 * GetCumulativeReward());
                }
                AddReward(-30.0f);
                indicator.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 0f;
                Stats.Instance.overtimeCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }

            // TODO: add check for if the rocket is slowing down when close to goal
            float distanceToGoal = Vector3.Distance(transform.localPosition, target.transform.localPosition);
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
            } else {
                AddReward(0.005f);
            }
            // outright fail if the current distance is 10m or more than the closest distance
            if (distanceToGoal > closestDistance + 30f) {
                Debug.Log("Failed because of going too far away from the platform");

                if (GetCumulativeReward() > 0) {
                    AddReward(-1 * GetCumulativeReward());
                }

                AddReward(-15.0f);
                indicator.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 0f;
                Stats.Instance.wander2Count++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }

            lastDistance = distanceToGoal;

            if (closestDistance < distanceToGoal) {
                closestDistance = distanceToGoal;
            }


            if (transform.localPosition.y > 5f){
                // transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);
                // add an offset of +90, 0 ,0 to make the rocket upright
                // transform.Rotate(90, 0, 0);
                //replace the above lines with RotateTowards to make it more smooth
                // point the top of the object to towards the target gradually
                Vector3 targetPosition = target.transform.position;
                Vector3 direction = targetPosition - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.Euler(90, 0, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);                
            }
            

            if (powerInput != null && massInput != null) {
                power = float.Parse(powerInput.text);
                rb.mass = float.Parse(massInput.text);
            }

            currentVelocity = (transform.localPosition - previousPosition) / Time.deltaTime;
            // Debug.Log($"{transform.localPosition} - {previousPosition} / {Time.deltaTime} = {currentVelocity}");
            previousPosition = transform.localPosition;

            // check if the rocket is above the goal
            if (transform.localPosition.y > (target.transform.localPosition.y + 10)) {
                Debug.Log("Failed because of going above the target");
                if (GetCumulativeReward() > 0) {
                    AddReward(-1 * GetCumulativeReward());
                }
                AddReward(-15.0f);
                indicator.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 0f;
                Stats.Instance.missedCount++;
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
                if (GetCumulativeReward() > 0) {
                    AddReward(-1 * GetCumulativeReward());
                }
                SetReward(-25.0f);
                indicator.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 0f;
                Stats.Instance.fallCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }



            if ((x_distance > 20 || z_distance > 20) && (lastXFromPlatform < x_distance || lastZFromPlatform < z_distance)) {
                // add some punishment because the rocket is wandering away from the platform
                AddReward(-0.01f);
            }

            if (x_distance > 200 || z_distance > 200) {
                Debug.Log("Failed because of going too far away from the platform");
                if (GetCumulativeReward() > 0) {
                    AddReward(-1 * GetCumulativeReward());
                }
                AddReward(-15.0f);
                target.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 1000f;
                Stats.Instance.wanderCount++;
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


            

            if (lastY > highestHeight) {
                highestHeight = lastY;
            }

        } else if (currentMode == "landing") {

            // keep the rocket upright
            // transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            // keep the rocket up gradually via slerp
            Vector3 direction = Vector3.up;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(90, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

            // zero out the local rotation
            // transform.localRotation = Quaternion.identity;

            if (powerInput != null && massInput != null) {
                power = float.Parse(powerInput.text);
                rb.mass = float.Parse(massInput.text);
            }

            currentVelocity = (transform.localPosition - previousPosition) / Time.deltaTime;
            // Debug.Log($"{transform.localPosition} - {previousPosition} / {Time.deltaTime} = {currentVelocity}");
            previousPosition = transform.localPosition;

            // check if the rocket is below the platform
            if (transform.localPosition.y < (target.transform.localPosition.y - 10)) {
                // Debug.Log("Failed because of going below the platform");
                AddReward(-15.0f);
                target.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 2000f;
                Stats.Instance.missedCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }

            // ignore in heuristic mode
            // bool isHeuristic = Academy.Instance.IsCommunicatorOn;
            if (transform.localPosition.y >= (lastY)) {
                // AddReward(-0.01f);

                // NOTE: ENABLE THIS IN TRAINING MODE ONLY
                // platform.GetComponent<MeshRenderer>().material = failMaterial;
                // lastY = 2000f;
                // Stats.Instance.fallCount++;
                // Stats.Instance.failureCount++;
                // AddReward(-20.0f);
                // EndEpisode();
            } else if (transform.localPosition.y < (lastY)) {
                // AddReward(0.01f);
            }

            if (transform.localPosition.y > 2000f) {
                // Debug.Log("Failed because of going too high");
                SetReward(-20.0f);
                target.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 2000f;
                Stats.Instance.fallCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }


            // check horizontal position of the rocket to platform
            // float x_distance = Mathf.Abs(transform.localPosition.x - target.transform.localPosition.x);
            // float z_distance = Mathf.Abs(transform.localPosition.z - target.transform.localPosition.z);

            if ((x_distance > 10 || z_distance > 10) && (lastXFromPlatform < x_distance || lastZFromPlatform < z_distance)) {
                // add some punishment because the rocket is wandering away from the platform
                // Debug.Log("Failed because of going too far away from the platform");
                // AddReward(-15.0f);
                // platform.GetComponent<MeshRenderer>().material = failMaterial;
                // lastY = 1000f;
                // Stats.Instance.wanderCount++;
                // Stats.Instance.failureCount++;
                // EndEpisode();
                AddReward(-0.01f);
            }

            if (x_distance > 400 || z_distance > 400) {
                // Debug.Log("Failed because of going too far away from the platform");
                AddReward(-15.0f);
                target.GetComponent<MeshRenderer>().material = failMaterial;
                lastY = 2000f;
                Stats.Instance.wanderCount++;
                Stats.Instance.failureCount++;
                EndEpisode();
            }


            // check the rotation if in the safe are to make it straight
            if (x_distance < 20 && z_distance < 20 && (transform.localRotation.x > 1 || transform.localRotation.z > 1)) {
                if (lastXRotation < transform.localRotation.x || lastZRotation < transform.localRotation.z) {
                    AddReward(-0.01f);
                }
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

    public void SwitchModes() {
        if (currentMode == "takeoff") {
            currentMode = "landing";
            behaviorParameters.Model = landingModel;
            target = GameObject.Find("Platform");
            targetSpeed = -15.0f;
            // set rocket velocity to zero
            rb.linearVelocity = Vector3.zero;
        } else {
            currentMode = "takeoff";
            behaviorParameters.Model = takeoffModel;
            target = GameObject.Find("Goal");
            targetSpeed = 50.0f;
        }
    }
}