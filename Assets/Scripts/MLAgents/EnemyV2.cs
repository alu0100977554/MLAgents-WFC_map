using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// Represents an agent of the shooter minigame
/// </summary>
public class EnemyV2 : Agent
{
    [Tooltip("Force to apply when moving")]
    public float _moveForce = 2e-6f;

    [Tooltip("Speed to rotate around the up axis")]
    public float _rotationSpeed = 75f;

    // Allows for smoother rotation changes
    private float _smoothRotationChange = 0f;

    public const int _maxHealth = 1;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool _trainingMode;

    // Rigidbody of the agent
    private Rigidbody _rigidbody;

    // The area where the agent is in
    [SerializeField]
    private ShooterArea _shooterArea;

    // The nearest agent to the agent
    private ShooterAgent _nearestAgent;

    // Distance to nearest agent
    private float _distanceToNearestAgent;

    /// <summary>
    /// Current amount of remaining health
    /// </summary>
    public int CurrentHealth { get; private set; }

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

        // If the playing is controlling the agent, max step is set to infinite
        if (!_trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agents and enemies when the episode begins and update the nearest agent
    /// </summary>
    public override void OnEpisodeBegin()
    {
        _shooterArea.ResetScene();
        UpdateNearestAgent();
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
        Vector3 toNearestAgent = _nearestAgent.transform.position - this.transform.position;

        // Observe the normalized vector to the nearest agent (3 observations)
        sensor.AddObservation(toNearestAgent.normalized);

        // Observe the angle between the agent's forward vector and the vector to nearest agent
        // Note: Vector3.Angle() returns always a value between 0 and 180
        sensor.AddObservation((Vector3.Angle(AgentForwardVector, toNearestAgent) + 180) % 180);

        // Get the distance to the nearest agent
        _distanceToNearestAgent = Vector3.Distance(_nearestAgent.transform.position, this.transform.position);

        // Observe that distance
        sensor.AddObservation(_distanceToNearestAgent);

        // 9 total observations
    }

    /// <summary>
    /// Called when action is received from either the player or the neural network
    /// 
    /// actions.ContinuousActions[i] represents:
    /// Index 0: move vector x (+1 = right, -1 = left)
    /// Index 1: move vector z (+1 = forward, -1 = backward)
    /// Index 2: rotate around Y axis (+1 = turn right, -1 = turn left) 
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
        continuousActions[0] = combined.x;
        continuousActions[1] = combined.z;
        continuousActions[2] = rotation;
    }

    /// <summary>
    /// The agent gets hit by a shot
    /// </summary>
    /// <param name="damage">Damage dealt by the shooter</param>
    public void GetShot(int damage)
    {
        // Subtract the damage from the agent's health
        CurrentHealth = (int)Mathf.Clamp(CurrentHealth, 0f, CurrentHealth - damage);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Enables the agent again
    /// </summary>
    public void Respawn()
    {
        // First, set the agent to active
        this.gameObject.SetActive(true);

        // Maximun number of attemps to respawn without colliding with another object
        int attemptsReamining = 100;
        Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_shooterArea._teamEnemiesBounds.min.x, _shooterArea._teamEnemiesBounds.max.x),
                                                1f,
                                                UnityEngine.Random.Range(_shooterArea._teamEnemiesBounds.min.z, _shooterArea._teamEnemiesBounds.max.z));

        // Check for collision
        while (Physics.CheckBox(potentialPosition, new Vector3(2f, 0.1f, 2f)) && attemptsReamining > 0) attemptsReamining--;

        this.transform.position = potentialPosition;

        // Reset healh
        CurrentHealth = _maxHealth;
    }

    /// <summary>
    /// Update the nearest agent to the agent
    /// </summary>
    private void UpdateNearestAgent()
    {
        bool allEnemiesInactive = true;
        foreach (ShooterAgent agent in _shooterArea._agents)
        {
            if (_nearestAgent == null && agent.isActiveAndEnabled)
            {
                _nearestAgent = agent;
                //allEnemiesInactive = false;
            }
            else if (agent.isActiveAndEnabled)
            {
                // Calculate distance to this agent and to the current nearest agent
                float distanceToagent = Vector3.Distance(this.transform.position, agent.transform.position);
                float distanceToNearestAgent = Vector3.Distance(this.transform.position, _nearestAgent.transform.position);

                // If nearest agent isn't active, or agent is closer, update the nearest agent
                if (!_nearestAgent.isActiveAndEnabled || distanceToagent < distanceToNearestAgent)
                {
                    _nearestAgent = agent;
                    //allEnemiesInactive = true;
                }
            }
        }

        //if (allEnemiesInactive)
        //_nearestAgent = null;
    }

    /// <summary>
    /// Disables the agent when its health drops below 1
    /// </summary>
    private void Die()
    {
        this.gameObject.SetActive(false);
    }

    private void Awake()
    {
        _shooterArea = GetComponentInParent<ShooterArea>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boundary")
        {
            SetReward(-0.5f);
            this.gameObject.SetActive(false);
        }

        if (other.gameObject.tag == "ShooterAgent")
        {
            SetReward(1f);
        }
    }
}
