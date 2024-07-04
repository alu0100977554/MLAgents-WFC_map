using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FSMAgent_patrol : FSMAgent
{
    [SerializeField]
    private bool _isInPatrollingArea;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        /*_animator = GetComponentInParent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _fsmArea = GetComponentInParent<FSMArea>();
        _spawningPoint = this.transform;

        // If the playing is controlling the agent, max step is set to infinite
        if (!_trainingMode) MaxStep = 0;*/
        base.Initialize();
    }

    /// <summary>
    /// Collect vector observation from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // The neural network works better with observation values between 0 and 1, so all
        // vectors and quaternions will be normalized

        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(this.transform.localRotation.normalized);

        // Get a vector from the agent to the nearest enemy
        Vector3 toPatrollingAreaCentre =_spawningPoint.transform.position - this.transform.position;

        // Observe the normalized vector to the centre of the patrolling area (3 observations)
        sensor.AddObservation(toPatrollingAreaCentre.normalized);

        // Observe the relative distance from the agent's position to the centre of the patrolling area (1 observation)
        sensor.AddObservation(toPatrollingAreaCentre.magnitude / _patrollingArea);

        // Observe if the agent is inside the patrolling area (1 observation)
        sensor.AddObservation(_isInPatrollingArea);

        // Observe the passive movement speed (1 observation)
        sensor.AddObservation(_passiveMovementSpeed / 0.05f);

        // 10 total observations
    }

    /// <summary>
    /// Called when action is received from either the player or the neural network
    /// 
    /// actions.ContinuousActions[i] represents:
    /// Index 0: rotate around Y axis (+1 = turn right, -1 = turn left)
    /// Index 1: move vector z (+1 = forward, -1 = backward)
    /// Index 2: change patrolling speed
    /// </summary>
    /// <param name="actions">The actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (_frozen)
        {
            return;
        }

        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;

        // Calculate rotation
        float rotationChange = actions.ContinuousActions[0];

        // Calculate smooth rotation changes
        _smoothRotationChange = Mathf.MoveTowards(_smoothRotationChange, rotationChange, 2f * Time.fixedDeltaTime);

        // Calculate new rotation based ont smoothed values
        float rotation = rotationVector.y + _smoothRotationChange * Time.fixedDeltaTime * _rotationSpeed;

        // Apply new rotation
        this.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

        // Calculate movement vector
        Vector3 movement = new Vector3(0f, 0f, actions.ContinuousActions[1]);

        // Calculate the passive movement speed
        _passiveMovementSpeed = Mathf.Clamp(_passiveMovementSpeed + (actions.ContinuousActions[2] / 20f), -0.01f, 0.05f);

        // Add force in the direction of the move vector
        //_rigidbody.AddForce(movement * _moveForce);
        //this.gameObject.transform.Translate(movement * Time.deltaTime);
        this.transform.position += _passiveMovementSpeed * AgentForwardVector + movement * Time.deltaTime * _moveForce;

        if (_isInPatrollingArea)
        {
            AddReward(0.01f);
        }
        else
        {
            AddReward(-0.005f);
        }
    }

    /// <summary>
    /// When behaviour type is set to "Heuristic only" on the agent's Behaviour Parameters,
    /// this function will be called. Its return values will be fed into
    /// <<see cref="OnActionReceived(ActionBuffers)"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">An output action array</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Create placeholders for all movement/turning
        Vector3 forward = Vector3.zero;
        Vector3 left = Vector3.zero;

        // Create placeholders for turning
        float rotation = 0f;

        // Forward/backward
        if (Input.GetKey(KeyCode.W)) forward = transform.forward;
        else if (Input.GetKey(KeyCode.S)) forward = -transform.forward;

        // Left/right
        //if (Input.GetKey(KeyCode.A)) left = -transform.right;
        //else if (Input.GetKey(KeyCode.D)) left = transform.right;

        // Convert keyboard inputs to movement and turning
        // All values should be between -1 and +1

        // Turn left/right
        if (Input.GetKey(KeyCode.Q)) rotation = -1f;
        else if (Input.GetKey(KeyCode.E)) rotation = 1f;

        // Combine the movement vectors and normalize
        Vector3 combined = (forward).normalized; ;

        // Apply movement
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = rotation;
        //continuousActions[1] = combined.x;
        continuousActions[1] = combined.z;
    }

    private void FixedUpdate()
    {
        if (!_frozen)
        {
            //Debug.Assert(_nearestEnemy != null, "Nearest enemy = NULL");
            _animator.SetFloat("distanceToEnemy", Vector3.Distance(this.transform.position, _nearestEnemy.transform.position));

            Debug.DrawLine(this.transform.position, _nearestEnemy.transform.position, Color.blue);

            if (Vector3.Distance(this.transform.position, _spawningPoint.position) < _patrollingArea)
            {
                _isInPatrollingArea = true;
                Debug.DrawLine(this.transform.position, _spawningPoint.transform.position, Color.green);
            }
            else
            {
                _isInPatrollingArea = false;
                Debug.DrawLine(this.transform.position, _spawningPoint.transform.position, Color.white);
            }
        }
    }
}
