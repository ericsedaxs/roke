using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

class AIExample : Agent 
{
    public override void OnEpisodeBegin()
    {
        // Reset the agent's position
        transform.position = new Vector3(0, 0, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the agent's position
        sensor.AddObservation(transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get the action
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Move the agent
        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime;

        // Reward the agent for moving
        AddReward(-0.001f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Get the action
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    public void OnCollisionEnter(Collision collision)
    {
        // Reward the agent for colliding with the goal
        if (collision.gameObject.CompareTag("Goal"))
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Punish the agent for colliding with the wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }
}