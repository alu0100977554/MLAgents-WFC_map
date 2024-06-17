using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FSMAgent_chase : Agent
{
    public Animator _animator;
    //public Patrol _patrolStateMachine;

    [Tooltip("Force to apply when moving")]
    public float _moveForce = 2e-6f;

    [Tooltip("Speed to rotate around the up axis")]
    public float _rotationSpeed = 75f;

    // Spawning point
    private Transform _spawningPoint;

    // The area around spawning point the agent is patrolling
    private float _patrollingArea = 25f;

    [Tooltip("Detection radius")]
    public float _detectionRadius = 20.0f;

    // The nearest enemy to the agent
    private FSMEnemy _nearestEnemy;

    // Distance to nearest enemy
    private float _distanceToNearestEnemy;

    [Tooltip("Whether the nearest enemy is in shooting range")]
    public bool _nearestEnemyInRange;

    // Distance to nearest enemy
    private bool _isInPatrollingArea;

    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool _trainingMode;

    // Rigidbody of the agent
    private Rigidbody _rigidbody;

    // The area where the agent is in
    public FSMArea _fsmArea;

    // Allows for smoother rotation changes
    private float _smoothRotationChange = 0f;

    /// <summary>
    /// A vector pointing straight forward out of the agent
    /// </summary>
    public Vector3 AgentForwardVector
    {
        get
        {
            return this.transform.forward;
        }
    }

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        _animator = GetComponentInParent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _fsmArea = GetComponentInParent<FSMArea>();
        _spawningPoint = this.transform;

        // If the playing is controlling the agent, max step is set to infinite
        if (!_trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agents and enemies when the episode begins and update the nearest enemy
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //_fsmArea.ResetScene();
        UpdateNearestEnemy();
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
        Vector3 toPatrollingAreaCentre = _spawningPoint.transform.position - this.transform.position;

        // Observe the normalized vector to the centre of the patrolling area (3 observations)
        sensor.AddObservation(toPatrollingAreaCentre.normalized);

        // Observe the relative distance from the agent's position to the centre of the patrolling area (1 obersvation)
        sensor.AddObservation(toPatrollingAreaCentre.magnitude / _patrollingArea);

        // 8 total observations
    }

    /// <summary>
    /// Called when action is received from either the player or the neural network
    /// 
    /// actions.ContinuousActions[i] represents:
    /// Index 1: rotate around Y axis (+1 = turn right, -1 = turn left)
    /// Index 1: move vector x (+1 = right, -1 = left)
    /// Index 2: move vector z (+1 = forward, -1 = backward)
    /// </summary>
    /// <param name="actions">The actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
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
        Vector3 movement = new Vector3(actions.ContinuousActions[1], 0f, actions.ContinuousActions[2]);

        // Add force in the direction of the move vector
        _rigidbody.AddForce(movement * _moveForce);

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
        if (Input.GetKey(KeyCode.A)) left = -transform.right;
        else if (Input.GetKey(KeyCode.D)) left = transform.right;

        // Convert keyboard inputs to movement and turning
        // All values should be between -1 and +1

        // Turn left/right
        if (Input.GetKey(KeyCode.Q)) rotation = -1f;
        else if (Input.GetKey(KeyCode.E)) rotation = 1f;

        // Combine the movement vectors and normalize
        Vector3 combined = (forward + left).normalized; ;

        // Apply movement
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = rotation;
        continuousActions[1] = combined.x;
        continuousActions[2] = combined.z;
    }

    /// <summary>
    /// Try to respawn the agent on a different safe position
    /// </summary>
    public void Respawn()
    {
        this.gameObject.SetActive(true);
        int attemptsReamining = 200;
        Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_fsmArea._teamAgentsBounds.min.x, _fsmArea._teamAgentsBounds.max.x),
                                                1f,
                                                UnityEngine.Random.Range(_fsmArea._teamAgentsBounds.min.z, _fsmArea._teamAgentsBounds.max.z));

        // 1º Check for collision with walls, enemies or other agents
        // 2º Check if the patrol area is within the boundaries (X axis)
        // 3º Check if the patrol area is within the boundaries (Z axis)
        while (Physics.CheckBox(potentialPosition, new Vector3(2f, 0.1f, 2f)) &&
               Mathf.Abs(_fsmArea._teamAgentsBounds.extents.x - potentialPosition.x) > _patrollingArea &&
               Mathf.Abs(_fsmArea._teamAgentsBounds.extents.z - potentialPosition.z) > _patrollingArea &&
               attemptsReamining > 0)
        {
            attemptsReamining--;
        }

        _spawningPoint.position = potentialPosition;
        this.transform.position = potentialPosition;
    }

    /// <summary>
    /// Update the nearest enemy to the agent
    /// </summary>
    private void UpdateNearestEnemy()
    {
        //bool allEnemiesInactive = true;
        foreach (FSMEnemy enemy in _fsmArea._enemies)
        {
            if (_nearestEnemy == null && enemy.isActiveAndEnabled)
            {
                _nearestEnemy = enemy;
                //allEnemiesInactive = false;
            }
            else if (enemy.isActiveAndEnabled)
            {
                // Calculate distance to this enemy and to the current nearest enemy
                float distanceToEnemy = Vector3.Distance(this.transform.position, enemy.transform.position);
                float distanceToNearestEnemy = Vector3.Distance(this.transform.position, _nearestEnemy.transform.position);

                // If nearest enemy isn't active, or enemy is closer, update the nearest enemy
                if (!_nearestEnemy.isActiveAndEnabled || distanceToEnemy < distanceToNearestEnemy)
                {
                    _nearestEnemy = enemy;
                    //allEnemiesInactive = true;
                }
            }
        }

        //if (allEnemiesInactive)
        //_nearestEnemy = null;
    }

    private void FixedUpdate()
    {
        _animator.SetFloat("distanceToEnemy", Vector3.Distance(this.transform.position, _nearestEnemy.transform.position));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boundary" || other.gameObject.tag == "Enemy")
        {
            SetReward(-0.5f);
            //this.gameObject.SetActive(false);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Debug.Log("Collision on wall");
            AddReward(-0.01f);
        }
    }
}
