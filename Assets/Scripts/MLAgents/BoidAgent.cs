using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BoidAgent : Agent
{
    public float _speed;
    public bool _turning = false;
    public Bounds _bounds;

    public float _updateTotalTime;
    public float _updateCurrentTime = 0.0f;

    public override void Initialize()
    {
        base.Initialize();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        _speed = Random.Range(BoidManager.BM._minSpeed, BoidManager.BM._maxSpeed);
        _bounds = new Bounds(BoidManager.BM.transform.position, BoidManager.BM._limits * 2);
        _updateTotalTime = Random.Range(8.0f, 15.0f);
    }

    public override void OnEpisodeBegin()
    {
        BoidManager.BM.ResetTarget();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.position);             // Adding Agent's position to Observation vector
        sensor.AddObservation(BoidManager.BM._target.transform.position);     // Adding Target's position to Observation vector
        //sensor.AddObservation(_predator.transform.localPosition);   // Adding Predator's position to Observation vector
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector3 moveToCentre = new Vector3(actions.ContinuousActions[0], actions.ContinuousActions[1], actions.ContinuousActions[2]);

        if (!_bounds.Contains(this.transform.position))
        {
            _turning = true;
        }
        else
        {
            _turning = false;
        }

        if (_turning)
        {
            Vector3 direction = BoidManager.BM.transform.position - this.transform.position;
            this.transform.rotation = Quaternion.Slerp(transform.rotation,                                      // Turn the agent back into the limits
                                                      Quaternion.LookRotation(direction),
                                                      BoidManager.BM._rotationSpeed * Time.deltaTime);
            AddReward(-0.001f);
        }
        else
        {
            _updateCurrentTime += Time.deltaTime;
            if (_updateCurrentTime >= _updateTotalTime)
            {
                _speed = Random.Range(BoidManager.BM._minSpeed, BoidManager.BM._maxSpeed);
                _updateCurrentTime = 0.0f;
            }

            if (Random.Range(0, 100) < 33)
            {
                ApplyDirectionRules(moveToCentre);
            }
        }

        if(Vector3.Distance(this.transform.position, BoidManager.BM._target.transform.position) < BoidManager.BM._neighbourDistance)
        {
            AddReward(0.001f);
        }

        this.transform.Translate(0f, 0f, _speed * Time.deltaTime);

        //transform.localPosition += new Vector3(moveX, 0, moveZ) * _moveSpeed;

        /*_distanceToPredator = Vector3.Distance(transform.localPosition, _predator.transform.localPosition);
        if (_distanceToPredator < _minDistanceToPredator)
        {
            AddReward(-0.05f);
        }*/

        AddReward(-0.05f);
    }

    void ApplyDirectionRules(Vector3 moveToCentre)
    {
        GameObject[] agents = BoidManager.BM._agents;

        Vector3 centreVector = BoidManager.BM._target.transform.position;
        Vector3 avoidVector = Vector3.zero;
        float agentSpeed = 0.01f;
        float neighbourDistance;
        int neighbourhoodSize = 0;

        foreach (GameObject neighbour in agents)
        {
            if (neighbour != this.gameObject)
            {
                neighbourDistance = Vector3.Distance(neighbour.transform.position, this.transform.position);
                if (neighbourDistance <= BoidManager.BM._neighbourDistance)
                {
                    AddReward(0.0001f);
                    centreVector += neighbour.transform.position + moveToCentre;
                    neighbourhoodSize++;

                    if (neighbourDistance < BoidManager.BM._minDistance)                                        // If another agent is too close, add the distance between them 
                    {                                                                                           // to the avoid vector
                        AddReward(-0.001f);
                        avoidVector += (this.transform.position - neighbour.transform.position);
                    }

                    BoidAgent neighbourAgent = neighbour.GetComponent<BoidAgent>();
                    agentSpeed += neighbourAgent._speed;
                }
            }
        }

        if (neighbourhoodSize > 0)
        {
            centreVector = centreVector / neighbourhoodSize + (BoidManager.BM._target.transform.position - this.transform.position);
            _speed = agentSpeed / neighbourhoodSize;
            if (_speed > BoidManager.BM._maxSpeed)
            {
                _speed = BoidManager.BM._maxSpeed;
            }
            Vector3 direction = (centreVector + avoidVector) - transform.position;
            if (direction != Vector3.zero)
            {
                this.transform.rotation = Quaternion.Slerp(transform.rotation,                                           // Every agent affects the rotation of the group
                                                      Quaternion.LookRotation(direction),
                                                      BoidManager.BM._rotationSpeed * Time.deltaTime);
            }
        }
    }

    // It uses player movement as heuristic mode for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        continuousActions[2] = Input.GetAxis("Mouse Y");
    }
}
