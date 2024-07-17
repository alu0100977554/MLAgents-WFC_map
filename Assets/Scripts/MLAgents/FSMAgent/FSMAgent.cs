using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FSMAgent : Agent
{
    [SerializeField]
    protected Animator _animator;
    //public Patrol _patrolStateMachine;

    [Tooltip("Force to apply when moving")]
    public float _moveForce = 1.5f;
    //public float _moveForce = 2e-6f;

    // Passive movement speed
    public float _passiveMovementSpeed = 0.05f;

    [Tooltip("Speed to rotate around the up axis")]
    public float _rotationSpeed = 75f;

    // Spawning point
    [SerializeField]
    protected Vector3 _spawningPoint;

    // The area around spawning point the agent is patrolling
    protected float _patrollingArea = 15f;

    [Tooltip("Detection radius")]
    public float _detectionRadius = 20.0f;

    // The nearest enemy to the agent
    protected FSMEnemy _nearestEnemy;

    // Distance to nearest enemy
    protected float _distanceToNearestEnemy;

    [Tooltip("Whether the nearest enemy is in shooting range")]
    public bool _nearestEnemyInRange;

    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool _trainingMode;

    // Rigidbody of the agent
    protected Rigidbody _rigidbody;

    // The area where the agent is in
    public FSMArea _fsmArea;

    // Allows for smoother rotation changes
    protected float _smoothRotationChange = 0f;

    // If the agent is frozen or not
    [SerializeField]
    protected bool _frozen = false;

    [SerializeField]
    //protected ScoreManager _scoreManager;

    protected int _score;

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
        //_spawningPoint = this.transform;

        // If the playing is controlling the agent, max step is set to infinite
        if (!_trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agents and enemies when the episode begins and update the nearest enemy
    /// </summary>
    public override void OnEpisodeBegin()
    {
        string gameData = JSONHelper.GameDataToJson(_score, _fsmArea._episodeLength);
        System.IO.File.WriteAllText("D:/Programas/Unity/Repos/MLAgents-WFC_map/Assets/Resources/gamedata/" + GetComponent<FSMAgent>().GetInstanceID() + ".json", gameData);
        _fsmArea.ResetScene();
        UpdateNearestEnemy();
        _score = 0;
    }

    /// <summary>
    /// Try to respawn the agent on a different safe position
    /// </summary>
    public void Respawn()
    {
        float y = 1.6f;
        if (_frozen)
        {
            y = -5f;
        }
        //this.gameObject.SetActive(true);
        int attemptsReamining = 200;
        Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_fsmArea._teamAgentsBounds.min.x, _fsmArea._teamAgentsBounds.max.x),
                                                y,
                                                UnityEngine.Random.Range(_fsmArea._teamAgentsBounds.min.z, _fsmArea._teamAgentsBounds.max.z));

        // 1º Check for collision with walls, enemies or other agents
        // 2º Check if the patrol area is within the boundaries (X axis)
        // 3º Check if the patrol area is within the boundaries (Z axis)
        while (Physics.CheckBox(potentialPosition, new Vector3(3f, 1.5f, 3f)) &&
               Mathf.Abs(_fsmArea._teamAgentsBounds.extents.x - potentialPosition.x) > _patrollingArea &&
               Mathf.Abs(_fsmArea._teamAgentsBounds.extents.z - potentialPosition.z) > _patrollingArea &&
               attemptsReamining > 0)
        {
            attemptsReamining--;
        }

        _spawningPoint = potentialPosition;
        this.transform.position = potentialPosition;
        //_frozen = false;
    }

    /// <summary>
    /// Freezes the agent
    /// </summary>
    public void Freeze()
    {
        this.transform.position = new Vector3(1000f, 1000f, 1000f);
        this.transform.rotation = Quaternion.identity;
        this._rigidbody.angularVelocity = Vector3.zero;
        //this.gameObject.SetActive(false);
        _frozen = true;
    }

    /// <summary>
    /// Unfreezes the agent
    /// </summary>
    /// <param name="transform">Returning position and rotation of the agent</param>
    public void Unfreeze(Transform transform)
    {
        this.transform.position = transform.position;
        this.transform.rotation = transform.rotation;
        this._rigidbody.angularVelocity = Vector3.zero;
        //this.gameObject.SetActive(true);
        _frozen = false;
    }

    /// <summary>
    /// Update the nearest enemy to the agent
    /// </summary>
    public void UpdateNearestEnemy()
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

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boundary" || other.gameObject.tag == "Enemy")
        {
            SetReward(-0.5f);
            //this.gameObject.SetActive(false);
            //Debug.Log("Trigger on boundary");
            Debug.Log("score = " + _score + ", before json");
            EndEpisode();
        }
    }

    protected void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Wall")
        {
            //Debug.Log("Collision on wall");
            AddReward(-0.01f);
        }
    }
}
