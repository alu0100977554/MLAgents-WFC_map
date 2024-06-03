using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterArea : MonoBehaviour
{
    // List of all agents within the area
    public List<ShooterAgent> _agents;

    // List of enemies within the area
    public List<Enemy> _enemies;

    public int _activeEnemies;

    [SerializeField]
    private GameObject _area;

    public Bounds _areaBounds;

    /// <summary>
    /// Resets all the agents and enemies in the area
    /// </summary>
    public void ResetScene()
    {
        foreach (ShooterAgent agent in _agents)
        {
            agent.Respawn();
        }
        foreach (Enemy enemy in _enemies)
        {
            enemy.Respawn();
        }
        _activeEnemies = _enemies.Count;
    }

    /// <summary>
    /// Called when the area wakes up
    /// </summary>
    private void Awake()
    {
        Debug.Assert(_area != null, "The area is NULL");
        _areaBounds = _area.GetComponent<Renderer>().bounds;
    }

    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        // Agents correspond with child index 0
        FindChildAgents(transform.GetChild(0));

        // Enemies correspond with child index 1
        FindChildEnemies(transform.GetChild(1));

        _activeEnemies = _enemies.Count;
    }

    /// <summary>
    /// Finds all agents in the area and adds it to the list
    /// </summary>
    /// <param name="agents">Parent of all agents</param>
    private void FindChildAgents(Transform agents)
    {
        for (int i = 0; i < agents.childCount; i++)
        {
            Transform child = agents.GetChild(i);
            if (child.CompareTag("ShooterAgent"))
            {
                ShooterAgent agent = child.GetComponent<ShooterAgent>();
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
                Enemy enemy = child.GetComponent<Enemy>();
                _enemies.Add(enemy);
            }
        }
    }

    /// <summary>
    /// Called every 0.02 seconds
    /// </summary>
    private void FixedUpdate()
    {
        if (_activeEnemies <= 0)
            ResetScene();
    }
}
