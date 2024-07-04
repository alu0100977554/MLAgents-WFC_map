using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private ShooterAgent _shooterAgent;

    private void Awake()
    {
        _shooterAgent = GetComponentInParent<ShooterAgent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Trigger enter with " + other.tag);
        // Check if it collides with an enemy
        // Substract agent's rewars if the shot fails
        if (other.tag == "FSMEnemy")
        {
            _shooterAgent.AddScore(other);
        }
        else
            _shooterAgent.AddReward(-0.05f);

        // Destroy the bullet after all type of collision
        Destroy(this.gameObject);
    }
}
