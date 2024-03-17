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

    private float _moveSpeed = 0.5f;
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
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int movement = actions.DiscreteActions[0];
        int moveX = 0;
        int moveZ = 0;
        switch(movement)
        {
            case 1:
                moveX = 1;
                break;
            case 2:
                moveX = -1;
                break;
            case 3:
                moveZ = 1;
                break;
            case 4:
                moveZ = -1;
                break;
            default:
                moveX = 0;
                moveZ = 0;
                break;
        }

        transform.localPosition += new Vector3(moveX, 0, moveZ) * _moveSpeed;
        // transform.Rotate(0, rotateY * _rotateSpeed, 0);

        AddReward(-0.01f);
    }

    // It uses player movement as heuristic mode for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActions[0] = 2;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 3;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 4;
        }
        else
        {
            discreteActions[0] = 0;
        }
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
