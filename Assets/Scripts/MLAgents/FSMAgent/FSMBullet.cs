using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMBullet : MonoBehaviour
{
    [SerializeField]
    private FSMAgent_shoot _shooterAgent;

    private void Awake()
    {
        _shooterAgent = GetComponentInParent<FSMAgent_shoot>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag + " has been shot");
        // Check if it collides with an enemy
        // Substract agent's rewars if the shot fails
        if (other.tag == "Enemy")
        {
            _shooterAgent.AddScore(other);
        }
        else
            _shooterAgent.AddReward(-0.05f);

        // Destroy the bullet after any type of collision
        Destroy(this.gameObject);
    }
}
