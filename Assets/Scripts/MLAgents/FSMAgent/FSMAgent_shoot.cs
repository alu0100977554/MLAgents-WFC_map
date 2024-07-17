using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FSMAgent_shoot : FSMAgent
{
    [SerializeField]
    private bool _isInPatrollingArea;

    [Tooltip("Whether the agent can shoot or not")]
    public bool _availableShot;

    [Tooltip("Damage per shot")]
    public int _shotDamage = 1;

    private bool _shootingTrajectoryBlocked;

    // Prefab of the bullet shot
    [SerializeField]
    private GameObject _bulletPrefab;

    public int _maxStepsBetweenShots = 25;

    private int _currentStepsBetweenShots = 0;

    [Tooltip("Max shooting range")]
    public float _maxShootingRange = 15.0f;

    // Shooting point (gun)
    public Transform _shootingPoint;

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
        _availableShot = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        _availableShot = true;
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

        // Get a vector from the agent to the nearest agent
        Vector3 toNearestEnemy = _nearestEnemy.transform.position - this.transform.position;

        // Observe the normalized vector to the nearest agent (3 observations)
        sensor.AddObservation(toNearestEnemy.normalized);

        // Observe that distance, relative (1 observation)
        sensor.AddObservation(toNearestEnemy.normalized.magnitude);

        // Observe the angle between the agent's forward vector and the vector to nearest enemy
        // Note: Vector3.Angle() returns always a value between 0 and 180 (1 observation)
        sensor.AddObservation((Vector3.Angle(AgentForwardVector, toNearestEnemy) + 180) % 180);

        // Observe if the shot is available (1 observation)
        sensor.AddObservation(_availableShot);

        // Observe if the nearest enemy is in shooting range (1 obersvation)
        sensor.AddObservation(_nearestEnemyInRange);

        // Observe if there is a clear shot to an enemy (1 observation)
        sensor.AddObservation(_shootingTrajectoryBlocked);

        // Observe the passive movement speed (1 observation)
        sensor.AddObservation(_passiveMovementSpeed / 0.05f);

        // 13 total observations
    }

    /// <summary>
    /// Called when action is received from either the player or the neural network
    /// 
    /// actions.ContinuousActions[i] represents:
    /// Index 0: rotate around Y axis (+1 = turn right, -1 = turn left)
    /// Index 1: move vector z (+1 = forward, -1 = backward)
    /// Index 2: change patrolling speed
    /// actions.DiscreteActions[i] represents:
    /// Index 0: shoot (+1 = shoot, 0: don't shoot)
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

        Vector3 toNearestEnemy = _nearestEnemy.transform.position - this.transform.position;
        if (toNearestEnemy.magnitude <= 15f)
        {
            AddReward(0.003f);
        }
        else
        {
            AddReward(-0.001f);
        }

        if ((Vector3.Angle(AgentForwardVector, toNearestEnemy) + 180) % 180 < 7.5f)
            AddReward(0.002f);
        else
            AddReward(-0.001f);

        // Try to shoot at nearest enemy
        if (actions.DiscreteActions[0] == 1 && _availableShot == true)
            Shoot();

        _fsmArea._episodeLength++;
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

        // Shoot
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.P) ? 1 : 0;
    }

    /// <summary>
    /// Increments score if the agent shots an enemy
    /// </summary>
    /// <param name="other">The object wich the bullet collides with</param>
    public void AddScore(Collider other)
    {
        Debug.Log("Adding score");
        // Call GetShot() function from Enemy and get a reward
        other.transform.GetComponent<FSMEnemy>().GetShot(_shotDamage);
        AddReward(0.1f);

        // Increment agent's score
        _score++;

        CheckRemainingEnemies();
    }

    /// <summary>
    /// Check for remaining active enemies after shooting one
    /// </summary>
    private void CheckRemainingEnemies()
    {
        // Ckeck if each enemy is active or not
        bool activeEnemies = false;
        foreach (FSMEnemy enemy in _fsmArea._enemies)
        {
            if (enemy.gameObject.activeSelf)
            {
                // Update nearest enemy if at least one enemy is active
                activeEnemies = true;
                UpdateNearestEnemy();
                break;
            }
        }

        // End episode if there is no enemies active
        if (!activeEnemies)
        {
            EndEpisode();
        }
    }

    /// <summary>
    /// The agent shoot a projectile
    /// </summary>
    private void Shoot()
    {
        Debug.DrawRay(_shootingPoint.position, AgentForwardVector, Color.red, 0.3f);
        _availableShot = false;

        // Fire the bullet
        GameObject bullet = Instantiate(_bulletPrefab, _shootingPoint.position, _shootingPoint.rotation, this.transform);
        bullet.GetComponent<Rigidbody>().AddForce(AgentForwardVector * 5000f);
    }

    private void FixedUpdate()
    {
        if (!_frozen)
        {
            //Debug.Assert(_nearestEnemy != null, "Nearest enemy = NULL");
            _animator.SetFloat("distanceToEnemy", Vector3.Distance(this.transform.position, _nearestEnemy.transform.position));

            Debug.DrawLine(this.transform.position, _nearestEnemy.transform.position, Color.red);

            // Check if the nearest enemy is in range
            if (_distanceToNearestEnemy < _maxShootingRange)
            {
                _nearestEnemyInRange = true;
            }
            else
            {
                _nearestEnemyInRange = false;
            }

            // Check if trajectory is clear
            int layerMask = 1 << LayerMask.NameToLayer("Enemy");
            if (Physics.Raycast(_shootingPoint.position, AgentForwardVector, out var hit, 200f, layerMask))
                _shootingTrajectoryBlocked = false;
            else
                _shootingTrajectoryBlocked = true;

            // Reload shot every _maxStepsBetweenShots * 0.02 s
            if (!_availableShot)
            {
                _currentStepsBetweenShots++;
                if (_currentStepsBetweenShots >= _maxStepsBetweenShots)
                {
                    _availableShot = true;
                    _currentStepsBetweenShots = 0;
                }
            }
        }  
    }
}
