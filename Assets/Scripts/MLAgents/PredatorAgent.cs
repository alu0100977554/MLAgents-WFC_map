using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PredatorAgent : Agent
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _rotateSpeed = 1f;
    private GameObject _floor;
    private Bounds _colliderBounds;

    public void Start()
    {
        _floor = GameObject.FindWithTag("Floor");
        _colliderBounds = _floor.GetComponent<Collider>().bounds;
    }

    public override void OnEpisodeBegin()
    {
        // Material defaultFloortMaterial = Resources.Load("FloorMaterial", typeof(Material)) as Material;
        // _floor.GetComponent<Renderer>().material = defaultFloortMaterial;

        if (_colliderBounds != null)
        {
            _targetTransform.localPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9));

            Vector3 tempPosition;
            do
            {
                tempPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9));
            } while (Physics.CheckBox(tempPosition, new Vector3(2f, 0.1f, 2f)));
            transform.localPosition = tempPosition;
        }
        else
            Debug.Log("Bounds for spawning agent and target not found");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);          // Adding Agent's position to Observation vector
        sensor.AddObservation(_targetTransform.localPosition);   // Adding Target's position to Observation vector
        sensor.AddObservation(_targetTransform.localRotation.y);
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
            // Material successFloorMaterial = Resources.Load("FloorMaterial_success", typeof(Material)) as Material;
            // _floor.GetComponent<Renderer>().material = successFloorMaterial;
            EndEpisode();
        }
        if (other.gameObject.tag == "Wall")
        {
            AddReward(-7.5f);
        }
        if (other.gameObject.tag == "Boundary")
        {
            AddReward(-10f);
            // Material failFloorMaterial = Resources.Load("FloorMaterial_fail", typeof(Material)) as Material;
            // _floor.GetComponent<Renderer>().material = failFloorMaterial;
            EndEpisode();
        }
    }
}
