using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Integrations.Match3;

public class ShooterAgent : Agent
{
    [Tooltip("Force to apply when moving")]
    public float _moveForce = 2e-6f;

    [Tooltip("Speed to rotate around the up axis")]
    public float _rotationSpeed = 75f;

    [Tooltip("Whether the agent can shoot or not")]
    public bool _availableShot;

    public int _maxStepsBetweenShots = 100;

    private int _currentStepsBetweenShots = 0;

    [Tooltip("Max shooting range")]
    public float _maxShootingRange = 5.0f;

    [Tooltip("Whether the nearest enemy is in shooting range")]
    public bool _nearestEnemyInRange;

    // Distance to nearest enemy
    private float _distanceToNearestEnemy;

    [Tooltip("Damage per shot")]
    public int _shotDamage = 1;

    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool _trainingMode;

    // Rigidbody of the agent
    private Rigidbody _rigidbody;

    // Shooting point (gun)
    public Transform _shootingPoint;

    // The area where the agent is in
    [SerializeField]
    private ShooterArea _shooterArea;

    // Maximum aiming angle to receive a reward
    public float _maxAimingAngle = 15f;

    // The nearest enemy to the agent
    private Enemy _nearestEnemy;

    // Allows for smoother rotation changes
    private float _smoothRotationChange = 0f;

    // Total score this episode
    [SerializeField]
    private ScoreManager _scoreManager;

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
        _rigidbody = GetComponent<Rigidbody>();
        _shooterArea = GetComponentInParent<ShooterArea>();
        _availableShot = true;

        // If the playing is controlling the agent, max step is set to infinite
        if (!_trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agents and enemies when the episode begins and update the nearest enemy
    /// </summary>
    public override void OnEpisodeBegin()
    {
        _shooterArea.ResetScene();
        UpdateNearestEnemy();
        _availableShot = true;
        _scoreManager = this.transform.GetChild(1).GetChild(0).GetComponentInChildren<ScoreManager>();
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
        Vector3 toNearestEnemy = _nearestEnemy.transform.position - this.transform.position;

        // Observe the normalized vector to the nearest enemy (3 observations)
        sensor.AddObservation(toNearestEnemy.normalized);

        // Observe the angle between the agent's forward vector and the vector to nearest enemy
        // Note: Vector3.Angle() returns always a value between 0 and 180
        sensor.AddObservation((Vector3.Angle(AgentForwardVector, toNearestEnemy) + 180) % 180);

        // Observe if the shot is available
        sensor.AddObservation(_availableShot);

        // Observe if the nearest enemy is in shooting range
        sensor.AddObservation(_nearestEnemyInRange);

        // Get the distance to the nearest enemy
        _distanceToNearestEnemy = Vector3.Distance(_nearestEnemy.transform.position, this.transform.position);

        // Observe that distance
        //sensor.AddObservation(_distanceToNearestEnemy);

        // 10 total observations
    }

    /// <summary>
    /// Called when action is received from either the player or the neural network
    /// 
    /// actions.ContinuousActions[i] represents:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector z (+1 = forward, -1 = backward)
    /// Index 2: rotate around Y axis (+1 = turn right, -1 = turn left) 
    /// 
    /// actions.DiscreteActions[i] represents:
    /// Index 1: shoot (+1 = shoot, 0: don't shoot) - por ahora no
    /// </summary>
    /// <param name="actions">The actions to take</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Calculate movement vector
        Vector3 movement = new Vector3(actions.ContinuousActions[0], 0f, actions.ContinuousActions[1]);

        // Add force in the direction of the move vector
        _rigidbody.AddForce(movement * _moveForce);

        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;

        // Calculate rotation
        float rotationChange = actions.ContinuousActions[2];

        // Calculate smooth rotation changes
        _smoothRotationChange = Mathf.MoveTowards(_smoothRotationChange, rotationChange, 2f * Time.fixedDeltaTime);

        // Calculate new rotation based ont smoothed values
        float rotation = rotationVector.y + _smoothRotationChange * Time.fixedDeltaTime * _rotationSpeed;

        // Apply new rotation
        this.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

        // Get the current aiming angle to the nearest enemy, if it is less than _maxAimingAngle,
        // add reward, else substract reward
        Vector3 toNearestEnemy = _nearestEnemy.transform.position - this.transform.position;
        float currentAimingAngle = Vector3.Angle(AgentForwardVector, toNearestEnemy);
        if (currentAimingAngle < _maxAimingAngle)
            AddReward(0.001f);
        else
            AddReward(-0.001f);

        // Try to shoot at nearest enemy
        if (actions.DiscreteActions[0] == 1 && _availableShot == true)
            Shoot();
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
        Vector3 combined = (forward + left).normalized;;

        // Apply movement
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = combined.x;
        continuousActions[1] = combined.z;
        continuousActions[2] = rotation;

        // Shoot
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.P) ? 1 : 0;
    }

    /// <summary>
    /// Try to respawn the agent on a different safe position
    /// </summary>
    public void Respawn()
    {
        this.gameObject.SetActive(true);
        int attemptsReamining = 100;
        Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_shooterArea._teamAgentsBounds.min.x, _shooterArea._teamAgentsBounds.max.x),
                                                1f,
                                                UnityEngine.Random.Range(_shooterArea._teamAgentsBounds.min.z, _shooterArea._teamAgentsBounds.max.z));

        // Check for collision
        while (Physics.CheckBox(potentialPosition, new Vector3(2f, 0.1f, 2f)) && attemptsReamining > 0) attemptsReamining--;

        this.transform.position = potentialPosition;
    }

    /// <summary>
    /// Update the nearest enemy to the agent
    /// </summary>
    private void UpdateNearestEnemy()
    {
        bool allEnemiesInactive = true;
        foreach (Enemy enemy in _shooterArea._enemies)
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

    /// <summary>
    /// The agent shoot a projectile
    /// </summary>
    private void Shoot()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        Debug.DrawRay(_shootingPoint.position, AgentForwardVector, Color.red, 0.3f);
        _availableShot = false;

        if (Physics.Raycast(_shootingPoint.position, AgentForwardVector, out var hit, 200f, layerMask))
        {
            // Call GetShot() function from Enemy and get a reward
            hit.transform.GetComponent<Enemy>().GetShot(_shotDamage);
            AddReward(0.1f);

            _scoreManager._score++;

            // Check number of active enemies. Update nearest enemy if there are remaining enemies,
            // end the episode if there are not any left
            bool activeEnemies = false;
            foreach (Enemy enemy in _shooterArea._enemies)
            {
                if (enemy.gameObject.activeSelf)
                {
                    activeEnemies = true;
                    UpdateNearestEnemy();
                    break;
                }
            }
            if (!activeEnemies) 
                EndEpisode();

            //UpdateNearestEnemy();
        }
        else
        {
            // Substract reward if the shot fails
            AddReward(-0.05f);
        }
    }

    /// <summary>
    /// Called on the first frame
    /// </summary>
    private void Start()
    {
        // Draw a sphere to see the shooting range
        //Gizmos.DrawWireSphere(this.transform.position, _maxShootingRange);
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        // Draw a line from the agent to the nearest enemy
        if (_nearestEnemy != null)
        {
            Debug.DrawLine(this.transform.position, _nearestEnemy.transform.position, Color.green);
        }
    }

    /// <summary>
    /// Called every 0.02 seconds
    /// </summary>
    private void FixedUpdate()
    {
        // Check if the nearest enemy is in range
        /*if (_distanceToNearestEnemy < _maxShootingRange)
        {
            _nearestEnemyInRange = true;
        }
        else
        {
            _nearestEnemyInRange = false;
        }*/

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

        // Avoid possible scenario where the nearest enemy may not be updated
        if (_nearestEnemy != null && !_nearestEnemy.isActiveAndEnabled)
        {
            UpdateNearestEnemy();
        }
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
}
