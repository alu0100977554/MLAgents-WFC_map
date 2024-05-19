using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    //private AreaPred _parentArea;
    public GameObject _targetPrefab;
    public GameObject _target;

    [Header("Boid Settings")]
    public static BoidManager BM;
    public GameObject _agentPrefab;
    public int _numOfAgents;
    public GameObject[] _agents;
    public Vector3 _limits = new Vector3(10f, 10f, 10f);
    //public Vector3 _targetPosition;

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
        //_parentArea = GetComponentInParent<AreaPred>();
        Vector3 targetPosition = this.transform.position + new Vector3(Random.Range(-_limits.x, _limits.x), 0, Random.Range(-_limits.z, _limits.z));
        _target = Instantiate(_targetPrefab, targetPosition, Quaternion.identity);

        _agents = new GameObject[_numOfAgents];
        for (int i = 0; i < _numOfAgents; i++)
        {
            Vector3 agentPosition = this.transform.position + new Vector3(Random.Range(-_limits.x, _limits.x), 0, Random.Range(-_limits.z, _limits.z));
            _agents[i] = Instantiate(_agentPrefab, agentPosition, Quaternion.identity);
        }
        BM = this;
    }

    public void ResetTarget()
    {
        _target.transform.position = this.transform.position + new Vector3(Random.Range(-_limits.x, _limits.x), 0, Random.Range(-_limits.z, _limits.z));
    }

    // Update is called once per frame
    /*void Update()
    {
        if (Random.Range(0, 100) < 20)
        {
            _targetPosition = this.transform.position + new Vector3(Random.Range(-_limits.x, _limits.x), 0, Random.Range(-_limits.z, _limits.z));
        }
    }*/
}

