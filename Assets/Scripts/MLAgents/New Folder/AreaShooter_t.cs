using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AreaShooter : MonoBehaviour
{
    //public static AreaShooter AS;
    [SerializeField] public GameObject[] _agents;
    [SerializeField] public GameObject _target;
    [SerializeField] private GameObject _floor;
    [SerializeField] public Bounds _floorBounds;

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
        {
            _floorBounds = _floor.GetComponent<Renderer>().bounds;
            Debug.Log("Floor and its bounds found");
        }   
        else
            Debug.Log("ERROR: Floor not found!");

        //AS = this;

        //ResetArea();
    }

    public GameObject GetAgent(int id)
    {
        return _agents[id];
    }

    public GameObject GetTarget()
    {
        return _target;
    }

    public void ResetArea()
    {
        ResetAgents();
        ResetTarget();
    }

    public void ResetAgents()
    {
        foreach (GameObject agent in _agents)
        {
            Vector3 tempAgentPosition;
            do
            {
                //tempAgentPosition = new Vector3(Random.Range(_floorBounds.min.x, _floorBounds.max.x), 0.8f, Random.Range(_floorBounds.min.z, _floorBounds.max.z));
                tempAgentPosition = new Vector3(Random.Range(_floorBounds.min.x, _floorBounds.max.x), agent.transform.position.y, (Random.Range(_floorBounds.min.z, _floorBounds.max.z)));
            } while (Physics.CheckBox(tempAgentPosition, new Vector3(2f, 0.1f, 2f)));
            agent.transform.position = tempAgentPosition;
        }
    }

    public void ResetTarget()
    {
        Vector3 tempTargetPosition;
        do
        {
            //tempTargetPosition = new Vector3(Random.Range(_floorBounds.min.x, _floorBounds.max.x), 1f, Random.Range(_floorBounds.min.z, _floorBounds.max.z));
            tempTargetPosition = new Vector3(Random.Range(_floorBounds.min.x, _floorBounds.max.x), _target.transform.position.y, (Random.Range(_floorBounds.min.z, _floorBounds.max.z)));
        } while (Physics.CheckBox(tempTargetPosition, new Vector3(2f, 0.1f, 2f)));
        _target.transform.position = tempTargetPosition;

    }
}
