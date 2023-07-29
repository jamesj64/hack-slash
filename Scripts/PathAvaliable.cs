using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathAvaliable : MonoBehaviour
{

    private NavMeshAgent agent;
    private Transform player;

    public bool pathAvaliable = true;

    public float timeSinceAvaliable = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        agent.isStopped = true;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(player.position);
        pathAvaliable = agent.pathStatus == NavMeshPathStatus.PathComplete;
        if (pathAvaliable)
        {
            timeSinceAvaliable += Time.deltaTime;
        } else
        {
            timeSinceAvaliable = 0;
        }
    }
}
