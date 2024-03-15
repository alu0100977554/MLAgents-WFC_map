using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class AreaPred : MonoBehaviour
{
    [SerializeField] private GameObject _agent;
    [SerializeField] private GameObject _target;
    private GameObject _floor;
    private Bounds _colliderBounds;

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
            _colliderBounds = _floor.GetComponent<Collider>().bounds;
        else
            Debug.Log("ERROR: Floor not found!");
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
            tempAgentPosition = new Vector3(Random.Range(_colliderBounds.min.x, _colliderBounds.max.x), 0.25f, Random.Range(_colliderBounds.min.x, _colliderBounds.max.x));
            //tempAgentPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9f));
            // tempAgentPosition = new Vector3(Random.Range(12f, 40f), 1.2f, Random.Range(-30f, -9f));
        } while (Physics.CheckBox(tempAgentPosition, new Vector3(2f, 0.1f, 2f)));
        _target.transform.localPosition = tempAgentPosition;
    }

    private void ResetTarget()
    {
        Vector3 tempTargetPosition;
        do
        {
            tempTargetPosition = new Vector3(Random.Range(_colliderBounds.min.x, _colliderBounds.max.x), 0.25f, Random.Range(_colliderBounds.min.x, _colliderBounds.max.x));
            //tempTargetPosition = new Vector3(Random.Range(6f, -6f), 0.25f, Random.Range(8f, -9f));
            // tempTargetPosition = new Vector3(Random.Range(12f, 40f), 1.2f, Random.Range(-30f, -9f));
        } while (Physics.CheckBox(tempTargetPosition, new Vector3(2f, 0.1f, 2f)));
        _target.transform.localPosition = tempTargetPosition;
    }
}
