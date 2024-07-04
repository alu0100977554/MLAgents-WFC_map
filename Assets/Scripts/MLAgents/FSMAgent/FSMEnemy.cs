using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// Represents an enemy of the shooter minigame
/// </summary>
public class FSMEnemy : Agent
{
    [Tooltip("Force to apply when moving")]
    public float _moveForce = 1.5f;

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
    private FSMArea _fsmArea;

    // The nearest agent to the agent
    private FSMAgent _nearestAgent;

    // Distance to nearest agent
    private float _distanceToNearestAgent;

    // The nearest target to the agent
    private GameObject _nearestTarget;

    // Distance to nearest target
    private float _distanceToNearestTarget;

    private int _mainLayer;
    private int _voidLayer;

    public int _maxUpdateSteps = 25;
    private int _currentUpdateSteps = 0;

    /// <summary>
    /// Current amount of remaining health
    /// </summary>
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _fsmArea = GetComponentInParent<FSMArea>();
        _mainLayer = GetComponent<Collider>().gameObject.layer;
        _voidLayer = LayerMask.NameToLayer("Void");                     // Void layer does not collide with anything

        // If the playing is controlling the agent, max step is set to infinite
        if (!_trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agents and enemies when the episode begins and update the nearest agent
    /// </summary>
    public override void OnEpisodeBegin()
    {
        _fsmArea.ResetScene();
        UpdateNearestAgent();
        UpdateNearestTarget();
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
        //Vector3 toNearestAgent = _nearestAgent.transform.position - this.transform.position;

        // Observe the normalized vector to the nearest agent (3 observations)
        //sensor.AddObservation(toNearestAgent.normalized);

        // Observe that distance, relative (1 observation)
        //sensor.AddObservation(toNearestAgent.normalized.magnitude);

        // Get a vector from the agent to the nearest target
        Vector3 toNearestTarget = _nearestTarget.transform.position - this.transform.position;

        // Observe the normalized vector to the nearest agent (3 observations)
        sensor.AddObservation(toNearestTarget.normalized);

        // Observe that distance, relative (1 observation)
        sensor.AddObservation(toNearestTarget.normalized.magnitude);

        // Observe the angle between the agent's forward vector and the vector to nearest target
        // Note: Vector3.Angle() returns always a value between 0 and 180
        sensor.AddObservation((Vector3.Angle(this.transform.forward, toNearestTarget) + 180) % 180);

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
        this.transform.position +=  movement * Time.deltaTime * _moveForce;

        AddReward(-0.0001f);
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
    /// The enemy gets hit by a shot
    /// </summary>
    /// <param name="damage">Damage dealt by the shooter</param>
    public void GetShot(int damage)
    {
        // Subtract the damage from the enemy's health
        CurrentHealth = (int)Mathf.Clamp(CurrentHealth, 0f, CurrentHealth - damage);

        if (CurrentHealth <= 0)
        {
            AddReward(-0.01f);
            this.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Enables the enemy again
    /// </summary>
    public void Respawn()
    {
        // First, set the enemy to active
        this.gameObject.SetActive(true);

        // Maximun number of attemps to respawn without colliding with another object
        int attemptsReamining = 100;
        Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_fsmArea._teamEnemiesBounds.min.x, _fsmArea._teamEnemiesBounds.max.x),
                                                1.4f,
                                                UnityEngine.Random.Range(_fsmArea._teamEnemiesBounds.min.z, _fsmArea._teamEnemiesBounds.max.z));

        // Check for collision
        while (Physics.CheckBox(potentialPosition, new Vector3(3f, 1.5f, 3f)) && attemptsReamining > 0) attemptsReamining--;

        this.transform.position = potentialPosition;

        // Reset healh
        CurrentHealth = _maxHealth;
    }

    /// <summary>
    /// Update the nearest agent to the agent
    /// </summary>
    private void UpdateNearestAgent()
    {
        //bool allEnemiesInactive = true;
        foreach (FSMAgent agent in _fsmArea._agents)
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
    /// Update the nearest agent to the agent
    /// </summary>
    private void UpdateNearestTarget()
    {
        //bool allEnemiesInactive = true;
        foreach (GameObject target in _fsmArea._targets)
        {
            if (_nearestTarget == null && target.activeSelf)
            {
                _nearestTarget = target;
                //allEnemiesInactive = false;
            }
            else if (target.activeSelf)
            {
                // Calculate distance to this target and to the current nearest target
                float distanceToTarget = Vector3.Distance(this.transform.position, target.transform.position);
                float distanceToNearestTarget = Vector3.Distance(this.transform.position, _nearestTarget.transform.position);

                // If nearest target isn't active, or target is closer, update the nearest target
                if (!_nearestTarget.activeSelf || distanceToTarget < distanceToNearestTarget)
                {
                    _nearestTarget = target;
                    //allEnemiesInactive = false;
                }
            }
        }

        /*if (allEnemiesInactive == true)
        { 
            _fsmArea.ResetScene();
        }*/
    }

    /// <summary>
    /// Hides the collider of the agent
    /// </summary>
    private void DisableCollider()
    {
        GetComponent<Collider>().gameObject.layer = _voidLayer;
    }

    private void EnableCollider()
    {
        GetComponent<Collider>().gameObject.layer = _mainLayer;
    }

    private void FixedUpdate()
    {
        _currentUpdateSteps++;

        // Avoid possible scenario where the nearest target may not be updated
        if (!_nearestTarget.activeSelf || _currentUpdateSteps >= _maxUpdateSteps)
        {
            _currentUpdateSteps = 0;
            UpdateNearestTarget();
        }

        //Debug.DrawLine(this.transform.position, _nearestAgent.transform.position, Color.red);
        Debug.DrawLine(this.transform.position, _nearestTarget.transform.position, Color.green);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Grass")
        {
            DisableCollider();
            //Debug.Log("Entering grass");
        }

        if (other.gameObject.tag == "Boundary")
        {
            SetReward(-0.01f);
            //this.gameObject.SetActive(false);
            EndEpisode();
        }

        if (other.gameObject.tag == "FSMAgent")
        {
            Debug.Log("Trigger with agent");
            AddReward(-0.01f);
            EndEpisode();
        }

        if (other.gameObject.tag == "Target")
        {
            Debug.Log("Trigger with target");
            SetReward(0.1f);
            other.gameObject.SetActive(false);
            UpdateNearestTarget();
            //EndEpisode();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Grass" && gameObject.layer == _voidLayer)
        {
            //Debug.Log("Exiting grass");
            EnableCollider();
        }
    }

    protected void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            Debug.Log("Collision on wall");
            AddReward(-0.01f);
        }

        if (other.gameObject.tag == "FSMAgent")
        {
            Debug.Log("Collision with agent");
            AddReward(-0.01f);
            EndEpisode();
        }
    }
}
