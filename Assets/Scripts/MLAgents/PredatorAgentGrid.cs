using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PredatorAgentGrid : Agent
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _rotateSpeed = 1f;
    private GameObject _floor;
    private Bounds _colliderBounds;
    private int _nWallCollisions = 0;
    private float _collisionTime = 0.0f;
    private float _maxCollisionTime = 5.0f;

    public void Start()
    {
        _floor = GameObject.FindWithTag("Floor");
        _colliderBounds = _floor.GetComponent<Collider>().bounds;
    }

    public override void OnEpisodeBegin()
    {
        if (_colliderBounds != null)
        {
            Vector3 tempTargetPosition;
            do
            {
                tempTargetPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9f));
                // tempTargetPosition = new Vector3(Random.Range(12f, 40f), 1.2f, Random.Range(-30f, -9f));
            } while (Physics.CheckBox(tempTargetPosition, new Vector3(2f, 0.1f, 2f)));
            _targetTransform.localPosition = tempTargetPosition;

            Vector3 tempAgentPosition;
            do
            {
                tempAgentPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9f));
                // tempAgentPosition = new Vector3(Random.Range(12f, 40f), 1.3f, Random.Range(-30f, -9f));
            } while (Physics.CheckBox(tempAgentPosition, new Vector3(2f, 0.1f, 2f)));
            transform.localPosition = tempAgentPosition;
        }
        else
            Debug.Log("Bounds for spawning agent and target not found");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);          // Adding Agent's position to Observation vector
        sensor.AddObservation(_targetTransform.localPosition);   // Adding Target's position to Observation vector
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
