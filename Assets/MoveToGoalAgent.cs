using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }
}
