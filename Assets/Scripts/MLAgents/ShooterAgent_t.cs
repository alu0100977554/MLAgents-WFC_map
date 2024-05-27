using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.Mathematics;

public class ShooterAgent_t : Agent
{
    /*public AreaShooter _parentArea;
    public GameObject _target;
    public Transform _shootingPoint;
    private Rigidbody _rigidBody;

    [Header("Agent Settings")]
    [Range(0.5f, 5f)]
    public float _moveSpeed;
    [Range(0.5f, 5f)]
    public float _rotateSpeed;
    [Range(0f, 180f)]
    public float _maxAimingAngle;

    [Range(20, 50)]
    public int _stepsBetweenShots;
    private int _currentStepsBetweenShots = 0;
    private bool _availableShot;
    [Range(1, 1)]
    public int _shotDamage;


    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = this.gameObject.GetComponent<Rigidbody>();
        _parentArea = GetComponentInParent<AreaShooter>();
        _target = _parentArea.GetTarget();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_availableShot)
        {
            _currentStepsBetweenShots++;
            if (_currentStepsBetweenShots >= _stepsBetweenShots)
            {
                _availableShot = true;
                _currentStepsBetweenShots = 0;
            }
        }
        Debug.DrawRay(_shootingPoint.position, this.transform.forward, Color.green, Time.fixedDeltaTime);
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void OnEpisodeBegin()
    {
        //AreaShooter.AS.ResetArea();
        _parentArea.ResetArea();
        _rigidBody.velocity = Vector3.zero;
        _availableShot = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(_availableShot);
        sensor.AddObservation(this.transform.position);
        //sensor.AddObservation(AreaShooter.AS._target.transform.position);
        //sensor.AddObservation(_target.transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //float shoot = actions.ContinuousActions[0];
        //float moveX = actions.ContinuousActions[1];
        //float moveZ = actions.ContinuousActions[2];
        float rotateY = actions.ContinuousActions[0];

        //Debug.Log(shoot);

        /*if (Mathf.RoundToInt(shoot) >= 1)
        {
            Shoot();
        }*/
        //this.transform.Translate(0f, 0f, moveZ * Time.deltaTime * _moveSpeed);
        //this.transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * _moveSpeed;
        //_rigidBody.velocity = new Vector3(0f, 0f, moveZ) * _moveSpeed * 10f;

        /*Transform movement = this.transform;
        movement.position = new Vector3(moveX, 0f, moveZ) * _moveSpeed;
        movement.Rotate(Vector3.up, rotateY * _rotateSpeed);
        _rigidBody.AddForce(movement.position, ForceMode.VelocityChange);
        //_rigidBody.Move(new Vector3(moveX, 0f, moveZ) * _moveSpeed, new Quaternion(0f, rotateY * _rotateSpeed, 0f, 1f));
        Debug.Log(_rigidBody.velocity);
        //_rigidBody.MovePosition(transform.position + transform.forward * moveZ * Time.deltaTime * _moveSpeed);
        this.transform.Rotate(Vector3.up, rotateY * _rotateSpeed);

        //Vector3 directionToTarget = AreaShooter.AS._target.transform.position - this.transform.position;
        Vector3 directionToTarget = _target.transform.position - this.transform.position;
        float angle = Vector3.Angle(this.transform.forward, directionToTarget);
        //Debug.Log(angle);
        float reward = Mathf.Min(-Mathf.Log(angle / (180 / 3), 10), 5f);             // angle <= 60º   <=>   reward >= 0
        AddReward(reward);                                                              
        /*if (Vector3.Distance(this.transform.position, AreaShooter.AS._target.transform.position) < 7f)
        {
            AddReward(0.0001f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //continuousActions[0] = Input.GetKey(KeyCode.P) ? 1.0f : 0.0f;
        continuousActions[0] = Input.GetAxis("Horizontal");
        //continuousActions[2] = Input.GetAxis("Vertical");
        //continuousActions[3] = Input.GetAxis("Horizontal");
        //continuousActions[1] = Input.GetKey(KeyCode.A) ? 1.0f : 0.0f;
        // transform.rotation.SetLookRotation();
    }

    private void Shoot()
    {
        if (!_availableShot)
            return;

        Vector3 direction = transform.forward;
        int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        Debug.DrawRay(_shootingPoint.position, direction, Color.green, 0.5f);

        if (Physics.Raycast(_shootingPoint.position, direction, out var hit, 200f, layerMask))
        {
            hit.transform.GetComponent<Enemy>().GetShot(_shotDamage, this);
        }
        else
        {
            AddReward(-1f);
        }
        _availableShot = false;
    }

    public void RegisterKill()
    {
        AddReward(5f);
        EndEpisode();
    }

    private void OnMouseDown()
    {
        Shoot();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boundary")
        {
            SetReward(-15f);
            EndEpisode();
        }
    }*/
}
