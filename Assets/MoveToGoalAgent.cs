using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _moveSpeed = 1f;
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);          // Adding Agent's position to Observation vector
        sensor.AddObservation(_targetTransform.position);   // Adding Target's position to Observation vector
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Once the movement is calculated, It is added to the current position
        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;
    }
}
