using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public float _speed;
    public bool _turning = false;
    public Bounds _bounds;

    public float _updateTotalTime;
    public float _updateCurrentTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        _speed = Random.Range(FlockManager.FM._minSpeed, FlockManager.FM._maxSpeed);
        _bounds = new Bounds(FlockManager.FM.transform.position, FlockManager.FM._limits * 2);
        _updateTotalTime = Random.Range(8.0f, 15.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(!_bounds.Contains(this.transform.position))
        {
            _turning = true;
        }
        else
        {
            _turning = false;
        }

        if(_turning)
        {
            Vector3 direction = FlockManager.FM.transform.position - this.transform.position;               
            this.transform.rotation = Quaternion.Slerp(transform.rotation,                                      // Turn the agent back into the limits
                                                      Quaternion.LookRotation(direction),
                                                      FlockManager.FM._rotationSpeed * Time.deltaTime);
        }
        else
        {
            _updateCurrentTime += Time.deltaTime;
            if (_updateCurrentTime >= _updateTotalTime)
            {
                _speed = Random.Range(FlockManager.FM._minSpeed, FlockManager.FM._maxSpeed);
                _updateCurrentTime = 0.0f;
            }

            if (Random.Range(0, 100) < 33)
            {
                ApplyDirectionRules();
            }
        }
        
        this.transform.Translate(0f, 0f, _speed * Time.deltaTime);
    }

    void ApplyDirectionRules()
    {
        GameObject[] agents = FlockManager.FM._agents;

        Vector3 centreVector = Vector3.zero;
        Vector3 avoidVector = Vector3.zero;
        float agentSpeed = 0.01f;
        float neighbourDistance;
        int neighbourhoodSize = 0;

        foreach(GameObject neighbour in agents)
        {
            if(neighbour != this.gameObject)
            {
                neighbourDistance = Vector3.Distance(neighbour.transform.position, this.transform.position);
                if(neighbourDistance <= FlockManager.FM._neighbourDistance)
                {
                    centreVector += neighbour.transform.position;
                    neighbourhoodSize++;

                    if(neighbourDistance < FlockManager.FM._minDistance)                                        // If another agent is too close, add the distance between them 
                    {                                                                                           // to the avoid vector
                        avoidVector += (this.transform.position - neighbour.transform.position);
                    }

                    Flock neighbourFlock = neighbour.GetComponent<Flock>();
                    agentSpeed += neighbourFlock._speed;
                }
            }
        }

        if(neighbourhoodSize > 0)
        {
            centreVector = centreVector / neighbourhoodSize + (FlockManager.FM._targetPosition - this.transform.position);
            _speed = agentSpeed / neighbourhoodSize;
            if(_speed > FlockManager.FM._maxSpeed)
            {
                _speed = FlockManager.FM._maxSpeed;
            }
            Vector3 direction = (centreVector + avoidVector) - transform.position;
            if (direction != Vector3.zero)
            {
                this.transform.rotation = Quaternion.Slerp(transform.rotation,                                           // Every agent affects the rotation of the group
                                                      Quaternion.LookRotation(direction),
                                                      FlockManager.FM._rotationSpeed * Time.deltaTime);
            }
        }
    }
}
