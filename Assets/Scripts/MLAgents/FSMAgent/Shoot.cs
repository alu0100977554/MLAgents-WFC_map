using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shoot : StateMachineBehaviour
{
    private GameObject _patrolAgent;
    private GameObject _chaseAgent;
    private GameObject _shootAgent;

    private void Awake()
    {

    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the position and rotation of the last agent, and update the new prefab index
        Transform agentTransform = animator.transform.GetChild(0).GetChild(animator.gameObject.GetComponent<FSMArea>()._prefabIndex);
        Rigidbody agentRigidbody = agentTransform.gameObject.GetComponent<Rigidbody>();
        animator.gameObject.GetComponent<FSMArea>()._prefabIndex = 2;

        // Get the patrol,chase and shoot agents
        _patrolAgent = animator.gameObject.transform.GetChild(0).GetChild(0).gameObject;
        _chaseAgent = animator.gameObject.transform.GetChild(0).GetChild(1).gameObject;
        _shootAgent = animator.gameObject.transform.GetChild(0).GetChild(2).gameObject;

        // Set the shoot agent's position and rotation
        _shootAgent.GetComponent<FSMAgent_shoot>().Unfreeze(agentTransform);
        _shootAgent.GetComponent<FSMAgent_shoot>().UpdateNearestEnemy();

        // Set the patrol and chase agents to inactive
        //_patrolAgent.SetActive(false);
        //_chaseAgent.SetActive(false);
        _patrolAgent.GetComponent<FSMAgent_patrol>().Freeze();
        _chaseAgent.GetComponent<FSMAgent_chase>().Freeze();

        /*_shootAgent.transform.position = agentTransform.position;
        _shootAgent.transform.rotation = agentTransform.rotation;
        _shootAgent.GetComponent<Rigidbody>().velocity = agentRigidbody.velocity;
        //_patrolAgent.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        _shootAgent.SetActive(true);*/
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log(NPC.gameObject.GetComponent<Rigidbody>().angularVelocity);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //NPC.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        //NPC.SetActive(false);
    }
}
