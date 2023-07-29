using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private int roundNum = 0;

    [SerializeField]
    private int curAlive = 0;

    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private List<Transform> spawns;

    [SerializeField]
    private float timeBetweenRounds;

    [SerializeField]
    private float timeBetweenSpawns;

    [SerializeField]
    private float destRad;

    private int frameNum = 0;

    private int maxAlive;

    private Transform[] alive;

    private Transform player;

    [SerializeField]
    private float spawnCirRad = 2f;

    [SerializeField]
    private TextMeshProUGUI RoundDisplay;

    public bool easyTest;

    private int enemiesPerRound(int round)
    {
        return easyTest ? round + 1 : round * round;
    }

    IEnumerator nextRound()
    {
        yield return new WaitForSecondsRealtime(timeBetweenRounds);
        //Debug.Log("Round: " + roundNum);
        if (curAlive > 0) yield break;
        roundNum++;
        RoundDisplay.text = "Round " + roundNum;
        curAlive = enemiesPerRound(roundNum);
        maxAlive = curAlive;
        alive = new Transform[curAlive];
        for (int i = 0; i < curAlive; i++)
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            Vector3 randomPoint = spawns[Random.Range(0, spawns.Count)].position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spawnCirRad;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                GameObject newGuy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
                newGuy.GetComponent<Enemy>().manager = gameObject;
                newGuy.GetComponent<NavMeshAgent>().avoidancePriority = i;
                alive[i] = newGuy.transform;
                yield return new WaitForSecondsRealtime(timeBetweenSpawns);
            } else
            {
                i--;
            }
        }
    }

    public void enemyDied()
    {
        curAlive = Mathf.Max(0, curAlive - 1);
        if (curAlive <= 0)
        {
            //Debug.Log(curAlive);
            StartCoroutine(nextRound());
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        StartCoroutine(nextRound());
    }

    private void Update()
    {
        if (curAlive == 0 || Time.frameCount % 4 != 0) return;
        frameNum = (frameNum + 1) % maxAlive;
        while (alive[frameNum] == null) frameNum = (frameNum + 1) % maxAlive;
        //float randAng = Random.Range(0f, 2f * Mathf.PI);
        //alive[frameNum].GetComponent<NavMeshAgent>().destination = player.position + destRad * new Vector3(Mathf.Cos(randAng), 0, Mathf.Sin(randAng));
        alive[frameNum].GetComponent<Enemy>().dest = player.position;
        //alive[frameNum].GetComponent<NavMeshAgent>().stoppingDistance = Mathf.SmoothStep(3f, 5f, (float) frameNum / curAlive);
        //Debug.Log(frameNum);
    }
}
