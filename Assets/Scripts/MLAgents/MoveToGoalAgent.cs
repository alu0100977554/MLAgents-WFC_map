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
    private Vector3 _lastPosition;

    private void Start()
    {
        _lastPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(70f, 0f), 1.5f, Random.Range(-1f, -38f));
        _targetTransform.localPosition = new Vector3(Random.Range(70f, 0f), 1.5f, Random.Range(-1f, -38f));
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

        // float currentDistanceToTarget = Mathf.Abs(Vector3.Distance(_targetTransform.position, transform.position));
        // float lastDistanceToTarget = Mathf.Abs(Vector3.Distance(_targetTransform.position, _lastPosition));
        // if (currentDistanceToTarget < lastDistanceToTarget)
        //     AddReward(0.01f);
        // else
        //    AddReward(-0.02f);
        // _lastPosition = transform.position;
        AddReward(-0.01f);
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
            AddReward(10f);
            EndEpisode();
        }
        if (other.TryGetComponent<Wall>(out Wall Wall))
        {
            AddReward(-7.5f);
        }
        if (other.TryGetComponent<Boundary>(out Boundary Boundary))
        {
            AddReward(-10f);
            EndEpisode();
        }
    }
}
