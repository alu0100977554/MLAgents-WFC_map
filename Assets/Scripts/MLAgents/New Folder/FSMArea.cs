using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMArea : MonoBehaviour
{
    // List of agents prefabs used in the state machine
    public List<GameObject> _agentPrefabs = new List<GameObject>();

    // Current prefab in use
    [HideInInspector]
    public int _prefabIndex = 0;

    // Number of agents
    public int _maxAgents = 1;

    // List of all agents within the area
    public List<FSMAgent> _agents;

    public GameObject _enemyPrefab;

    // Number of enemies
    public int _maxEnemies = 1;

    // List of enemies within the area
    public List<FSMEnemy> _enemies;

    public GameObject _targetPrefab;

    // Number of targets
    public int _maxTargets = 5;

    // List of targets
    public List<GameObject> _targets;

    [SerializeField]
    private GameObject _area;

    public Bounds _areaBounds;
    public Bounds _teamAgentsBounds;
    public Bounds _teamEnemiesBounds;

    /// <summary>
    /// Resets all the agents and enemies in the area
    /// </summary>
    public void ResetScene()
    {
        foreach (FSMAgent agent in _agents)
        {
            agent.Respawn();
        }
        _agents[1].Freeze();
        _agents[2].Freeze();
        foreach (FSMEnemy enemy in _enemies)
        {
            enemy.Respawn();
        }
        ResetTargets();
    }

    /// <summary>
    /// Called when the area wakes up
    /// </summary>
    private void Awake()
    {
        Debug.Assert(_area != null, "The area is NULL");
        _areaBounds = _area.GetComponent<Renderer>().bounds;
        _teamAgentsBounds = _area.transform.GetChild(0).GetComponent<Renderer>().bounds;
        _teamEnemiesBounds = _area.transform.GetChild(1).GetComponent<Renderer>().bounds;

        // Instantiate _maxAgents agents as childs of Agents
        for (int i = 0; i < _maxAgents; i++)
        {
            // Spawn new FSMAgent_patrol
            GameObject newPatrolAgent = Instantiate(_agentPrefabs[0], this.transform.position, Quaternion.identity, this.transform.GetChild(0));
            _agents.Add(newPatrolAgent.GetComponent<FSMAgent>());
            _prefabIndex = 0;

            GameObject newChaseAgent = Instantiate(_agentPrefabs[1], this.transform.position, Quaternion.identity, this.transform.GetChild(0));
            _agents.Add(newChaseAgent.GetComponent<FSMAgent>());
            newChaseAgent.GetComponent<FSMAgent_chase>().Freeze();
            //_prefabIndex = 1;

            GameObject newShootAgent = Instantiate(_agentPrefabs[2], this.transform.position, Quaternion.identity, this.transform.GetChild(0));
            _agents.Add(newShootAgent.GetComponent<FSMAgent>());
            newShootAgent.GetComponent<FSMAgent_shoot>().Freeze();
            //_prefabIndex = 2;
        }

        // Instantiate _maxEnemies enemies as childs of Enemies
        for (int i = 0; i < _maxEnemies; i++)
        {
            GameObject newEnemy = Instantiate(_enemyPrefab, this.transform.position, Quaternion.identity, this.transform.GetChild(1));
            _enemies.Add(newEnemy.GetComponent<FSMEnemy>());
        }

        for (int i = 0; i < _maxTargets; i++)
        {
            GameObject newTarget = Instantiate(_targetPrefab, this.transform.position, Quaternion.identity, this.transform.GetChild(2));
            _targets.Add(newTarget);
        }
    }

    /*/// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        // Agents correspond with child index 0
        FindChildAgents(transform.GetChild(0));

        // Enemies correspond with child index 1
        FindChildEnemies(transform.GetChild(1));

        _activeEnemies = _enemies.Count;
    }*/

    /// <summary>
    /// Finds all agents in the area and adds it to the list
    /// </summary>
    /// <param name="agents">Parent of all agents</param>
    private void FindChildAgents(Transform agents)
    {
        for (int i = 0; i < agents.childCount; i++)
        {
            Transform child = agents.GetChild(i);
            if (child.CompareTag("FSMAgent"))
            {
                FSMAgent_patrol agent = child.GetComponent<FSMAgent_patrol>();
                _agents.Add(agent);
            }
        }
    }

    /// <summary>
    /// Finds all enemies in the area and adds it to the list
    /// </summary>
    /// <param name="enemies">Parent of all enemies</param>
    private void FindChildEnemies(Transform enemies)
    {
        for (int i = 0; i < enemies.childCount; i++)
        {
            Transform child = enemies.GetChild(i);
            if (child.CompareTag("Enemy"))
            {
                FSMEnemy enemy = child.GetComponent<FSMEnemy>();
                _enemies.Add(enemy);
            }
        }
    }

    /// <summary>
    /// Called every 0.02 seconds
    /// </summary>
    /*private void FixedUpdate()
    {
        if (_activeEnemies <= 0)
            ResetScene();
    }*/

    private void ResetTargets()
    {
        foreach (GameObject target in _targets)
        {
            // First, set the enemy to active
            target.SetActive(true);

            // Maximun number of attemps to respawn without colliding with another object
            int attemptsReamining = 100;
            Vector3 potentialPosition = new Vector3(UnityEngine.Random.Range(_areaBounds.min.x, _areaBounds.max.x),
                                                    1.4f,
                                                    UnityEngine.Random.Range(_areaBounds.min.z, _areaBounds.max.z));

            // Check for collision
            while (Physics.CheckBox(potentialPosition, new Vector3(3f, 1.5f, 3f)) && attemptsReamining > 0) attemptsReamining--;

            target.transform.position = potentialPosition;
        }
    }
}
