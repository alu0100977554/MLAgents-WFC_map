using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chase : StateMachineBehaviour
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
        animator.gameObject.GetComponent<FSMArea>()._prefabIndex = 1;

        // Get the patrol,chase and shoot agents
        _patrolAgent = animator.gameObject.transform.GetChild(0).GetChild(0).gameObject;
        _chaseAgent = animator.gameObject.transform.GetChild(0).GetChild(1).gameObject;
        _shootAgent = animator.gameObject.transform.GetChild(0).GetChild(2).gameObject;

        // Set the patrol agent's position and rotation
        _chaseAgent.GetComponent<FSMAgent_chase>().Unfreeze(agentTransform);
        _chaseAgent.GetComponent<FSMAgent_chase>().UpdateNearestEnemy();

        // Set the chase patrol and shoot to inactive
        //_patrolAgent.SetActive(false);
        //_shootAgent.SetActive(false);
        _patrolAgent.GetComponent<FSMAgent_patrol>().Freeze();
        _shootAgent.GetComponent<FSMAgent_shoot>().Freeze();

        /*_chaseAgent.transform.position = agentTransform.position;
        _chaseAgent.transform.rotation = agentTransform.rotation;
        _chaseAgent.GetComponent<Rigidbody>().velocity = agentRigidbody.velocity;
        //_chaseAgent.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        //_chaseAgent.SetActive(true);
        _chaseAgent.GetComponent<FSMAgent_chase>()._frozen = false;*/
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _chaseAgent.GetComponent<FSMAgent_chase>().UpdateNearestEnemy();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
