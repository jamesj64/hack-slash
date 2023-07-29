using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{

    [SerializeField]
    private Transform playerSpawn;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.transform.position = playerSpawn.position;
            } else
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null) enemy.Die();
            }
        }
    }
}
