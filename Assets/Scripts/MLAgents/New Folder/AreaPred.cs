using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AreaPred : MonoBehaviour
{
    [SerializeField] private GameObject _agent;
    [SerializeField] private GameObject _target;
    [SerializeField] private GameObject _floor;
    [SerializeField] private Bounds _floorBounds;

    public void Start()
    {
        foreach (Transform transform in this.transform)
        {
            if(transform.CompareTag("Floor"))
            {
                _floor = transform.gameObject;
                break;
            }
        }

        if (_floor != null)
            _floorBounds = _floor.GetComponent<Renderer>().bounds;
        else
            Debug.Log("ERROR: Floor not found!");

        ResetArea();
    }

    public GameObject GetAgent()
    {
        return _agent;
    }

    public GameObject GetTarget()
    {
        return _target;
    }

    public void ResetArea()
    {
        ResetAgent();
        ResetTarget();
    }

    private void ResetAgent()
    {
        Vector3 tempAgentPosition;
        do
        {
            // tempAgentPosition = new Vector3(_floorBounds.center.x, 0.8f,_floorBounds.max.x));
            tempAgentPosition = new Vector3(Random.Range(_floorBounds.min.x, _floorBounds.max.x), 0.8f, Random.Range(_floorBounds.min.z, _floorBounds.max.z));
            //tempAgentPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9f));
            // tempAgentPosition = new Vector3(Random.Range(12f, 40f), 1.2f, Random.Range(-30f, -9f));
        } while (Physics.CheckBox(tempAgentPosition, new Vector3(2f, 0.1f, 2f)));
        _agent.transform.position = tempAgentPosition;
    }

    private void ResetTarget()
    {
        Vector3 tempTargetPosition;
        do
        {
            tempTargetPosition = new Vector3(Random.Range(_floorBounds.min.x, _floorBounds.max.x), 1f, Random.Range(_floorBounds.min.z, _floorBounds.max.z));
            //tempTargetPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9f));
            // tempTargetPosition = new Vector3(Random.Range(12f, 40f), 1.2f, Random.Range(-30f, -9f));
        } while (Physics.CheckBox(tempTargetPosition, new Vector3(2f, 0.1f, 2f)));
        _target.transform.position = tempTargetPosition;
    }
}
