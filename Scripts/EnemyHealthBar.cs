using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{

    /*private void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }*/

    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform.position);
        }
    }
}
