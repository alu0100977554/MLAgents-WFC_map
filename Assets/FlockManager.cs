using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager FM;
    public GameObject _agentPrefab;
    public int _numOfAgents;
    public GameObject[] _agents;
    public Vector3 _limits = new Vector3(10, 10, 10);
    public Vector3 _targetPosition = Vector3.zero;

    [Header("Agent Settings")]
    [Range(0.2f, 4.5f)]
    public float _minSpeed;
    [Range(0.5f, 5.0f)]
    public float _maxSpeed;
    [Range(1.0f, 10.0f)]
    public float _neighbourDistance;
    [Range(1.0f, 5.0f)]
    public float _minDistance;
    [Range(1.0f, 5.0f)]
    public float _rotationSpeed;


    // Start is called before the first frame update
    void Start()
    {
        _agents = new GameObject[_numOfAgents];
        for (int i = 0; i < _numOfAgents; i++)
        {
            Vector3 agentPosition = this.transform.position + new Vector3(Random.Range(-_limits.x, _limits.x), 0, Random.Range(-_limits.z, _limits.z));
            _agents[i] = Instantiate(_agentPrefab, agentPosition, Quaternion.identity);
        }
        FM = this;
        _targetPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Random.Range(0, 100) < 20)
        {
            _targetPosition = this.transform.position + new Vector3(Random.Range(-_limits.x, _limits.x), 0, Random.Range(-_limits.z, _limits.z));
        }
    }
}
