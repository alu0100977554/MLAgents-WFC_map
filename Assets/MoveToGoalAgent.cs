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

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);          // Adding Agent's position to Observation vector
        sensor.AddObservation(_targetTransform.localPosition);   // Adding Target's position to Observation vector
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Once the movement is calculated, It is added to the current position
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;
    }

    // It uses player movement as heuristic mode for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Target>(out Target target))
        {
            SetReward(1f);
            EndEpisode();
        }
        if (other.TryGetComponent<Boundary>(out Boundary boundary))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}
