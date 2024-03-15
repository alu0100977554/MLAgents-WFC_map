using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PredatorAgentGrid : Agent
{
    private AreaPred _parentArea;
    private GameObject _target;

    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _rotateSpeed = 1f;

    private int _nWallCollisions = 0;
    private float _collisionTime = 0.0f;
    private float _maxCollisionTime = 5.0f;

    public override void Initialize()
    {
        base.Initialize();
        _parentArea = GetComponentInParent<AreaPred>();
        _target = _parentArea.GetTarget();
    }

    public override void OnEpisodeBegin()
    {
        _parentArea.ResetArea();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);          // Adding Agent's position to Observation vector
        sensor.AddObservation(_target.transform.localPosition);   // Adding Target's position to Observation vector
        sensor.AddObservation(transform.localRotation.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotateY = actions.ContinuousActions[2];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * _moveSpeed;
        transform.Rotate(0, rotateY * _rotateSpeed, 0);

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
        if (other.gameObject.tag == "Target")
        {
            AddReward(10f);
            EndEpisode();
        }

        if (other.gameObject.tag == "Boundary")
        {
            AddReward(-10f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Debug.Log("Collision on wall");
            AddReward(-5f);

            if (_nWallCollisions >= 10)
            {
                _nWallCollisions = 0;
                Debug.Log("Deberia reiniciarse");
                AddReward(-10f);
                EndEpisode();
            }
            else
                _nWallCollisions++;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        _collisionTime += Time.deltaTime;
        if (_collisionTime >= _maxCollisionTime)
        {
            _collisionTime = 0.0f;
            AddReward(-10f);
            EndEpisode();
        }
    }
}
