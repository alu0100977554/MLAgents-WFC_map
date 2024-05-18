using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PreyAgent : Agent
{
    private AreaPred _parentArea;
    private GameObject _target;
    private GameObject _predator;

    private float _moveSpeed = 0.24f;

    [SerializeField] private float _minDistanceToPredator = 3.0f;
    [SerializeField] private float _distanceToPredator;

    private int _mainLayer;
    private int _voidLayer;

    private int _nWallCollisions = 0;
    private float _collisionTime = 0.0f;
    private float _maxCollisionTime = 50f;

    private float _maxHidingTime = 50f;
    private float _hidingTime = 0f;

    public override void Initialize()
    {
        base.Initialize();
        _parentArea = GetComponentInParent<AreaPred>();
        _target = _parentArea.GetTarget();
        _predator = _parentArea.GetAgent(0);
        _mainLayer = GetComponent<Collider>().gameObject.layer;
        _voidLayer = LayerMask.NameToLayer("Void");             // Void layer does not collide with anything
    }

    public override void OnEpisodeBegin()
    {
        _parentArea.ResetArea();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);             // Adding Agent's position to Observation vector
        sensor.AddObservation(_target.transform.localPosition);     // Adding Target's position to Observation vector
        sensor.AddObservation(_predator.transform.localPosition);   // Adding Predator's position to Observation vector
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int movement = actions.DiscreteActions[0];
        int moveX = 0;
        int moveZ = 0;
        switch (movement)
        {
            case 1:
                moveX = 1;
                moveZ = 0;
                break;
            case 2:
                moveX = -1;
                moveZ = 0;
                break;
            case 3:
                moveX = 0;
                moveZ = 1;
                break;
            case 4:
                moveX = 0;
                moveZ = -1;
                break;
            default:
                moveX = 0;
                moveZ = 0;
                break;
        }

        transform.localPosition += new Vector3(moveX, 0, moveZ) * _moveSpeed;

        _distanceToPredator = Vector3.Distance(transform.localPosition, _predator.transform.localPosition);
        if (_distanceToPredator < _minDistanceToPredator)
        {
            AddReward(-0.05f);
        }

        AddReward(0.005f);
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
        if (other.gameObject.tag == "Grass")
        {
            DisableCollider();
            Debug.Log("Entering grass");
        }

        if (other.gameObject.tag == "Target")
        {
            Debug.Log("Entering target");
            SetReward(10f);
            EndEpisode();
        }

        if (other.gameObject.tag == "Boundary" || other.gameObject.tag == "PredatorAgent")
        {
            Debug.Log("Exiting limits");
            SetReward(-10f);
            EndEpisode();
        }
    }

    /*private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Grass")
        {
            DisableCollider();
            Debug.Log("Entering grass");
        }
    }*/

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Grass" && gameObject.layer == _voidLayer)
        {
            Debug.Log("Exiting grass");
            EnableCollider();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            //Debug.Log("Collision on wall");
            AddReward(-3f);

            if (_nWallCollisions >= 10)
            {
                _nWallCollisions = 0;
                Debug.Log("Deberia reiniciarse");
                SetReward(-10f);
                EndEpisode();
            }
            else
                _nWallCollisions++;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            //Debug.Log("On Collision stay (Wall)");
            _collisionTime += Time.deltaTime;
            if (_collisionTime >= _maxCollisionTime)
            {
                _collisionTime = 0.0f;
                Debug.Log("Superado el tiempo de colision");
                SetReward(-10f);
                EndEpisode();
            }
        }   
    }

    private void DisableCollider()
    {
        GetComponent<Collider>().gameObject.layer = _voidLayer;
    }

    private void EnableCollider()
    {
        GetComponent<Collider>().gameObject.layer = _mainLayer;
    }

    private void StartTimer()
    {
        _hidingTime += Time.deltaTime;
        Debug.Log(_hidingTime);
        if (_hidingTime >= _maxHidingTime)
        {
            EnableCollider();
            Debug.Log("Exiting grass");
        }
    }
}
