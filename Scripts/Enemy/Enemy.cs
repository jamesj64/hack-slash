using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    private GameObject bloodEffect;

    [SerializeField]
    private float knockback;

    [SerializeField]
    private float knockbackTime;

    private bool hitCooldown = false;

    public float health;

    [SerializeField]
    private float maxHealth;

    [SerializeField]
    private Slider healthSlider;

    [HideInInspector]
    public GameObject player;

    [SerializeField]
    private float timeToResume;

    [SerializeField]
    private float scoreOnDeath;

    private bool isWaiting = false;

    private Rigidbody rb;

    private NavMeshAgent agent;

    private Animator animator;

    [SerializeField]
    private LayerMask ground;

    private bool targeted;

    [SerializeField]
    private GameObject visualOption;

    [SerializeField]
    private Material targetedMat;

    private Material origMat;

    [Header("Audio")]
    [SerializeField]
    private List<AudioSource> hitSources;

    [HideInInspector]
    public GameObject manager;

    [Header("Attack Logic")]
    [SerializeField]
    private float attackDist;

    [SerializeField]
    private float startupTime;

    [SerializeField]
    private float attackCooldownTime;

    private bool attackCooldown = false;

    private Weapon weapon;

    [SerializeField]
    private List<string> attacks;

    private string currentAttack = null;

    public Vector3 dest;

    private NavMeshObstacle obstacle;

    private float distThing;

    private bool noPath = false;

    private PathAvaliable pathGuy;

    private bool wasAttacked = false;

    /*public delegate void DeathAction(GameObject dead, float scoreIncrease);
    public static event DeathAction OnDeath;*/

    public static event Action<GameObject, float> OnDeath;

    //FIX ENEMINES SNAPPING TO GROUND AFTER BEING LAUNCHED

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
        animator.SetBool("isWalking", true);
        origMat = visualOption.GetComponent<SkinnedMeshRenderer>().materials.First();
        weapon = GetComponent<Weapon>();
        obstacle = GetComponent<NavMeshObstacle>();
        pathGuy = GameObject.Find("PathStatusGuy").GetComponent<PathAvaliable>();

        healthSlider.maxValue = maxHealth;
        health = maxHealth;
        healthSlider.value = health;
    }

    public void ResetAttack()
    {
        attackCooldown = false;
    }

    void Attack()
    {
        if (health <= 0 || attacks.Count == 0) return;
        currentAttack = attacks[UnityEngine.Random.Range(0, attacks.Count)];
        animator.SetTrigger(currentAttack);
        Invoke(nameof(ResetAttack), attackCooldownTime + UnityEngine.Random.Range(0f, 3f));
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            animator.SetTrigger("isDead");
            return;
        }

        //if (weapon.attacking) firstAttack = true;

        if (hitCooldown) return;

        if (!agent.enabled && rb.velocity.magnitude < 0.5f && Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.3f, ground) && wasAttacked)
        {
            wasAttacked = false;
            noPath = false;
            obstacle.enabled = false;
            agent.enabled = true;
            agent.isStopped = false;
        }


        if (wasAttacked) return;

        float realDist = Vector3.Distance(transform.position, player.transform.position);

        if (agent.enabled)
        {
            distThing = agent.remainingDistance;
            if (agent.pathStatus != NavMeshPathStatus.PathComplete && !noPath && !pathGuy.pathAvaliable)
            {
                NavMeshHit hit;
                if (agent.FindClosestEdge(out hit))
                {
                    agent.SetDestination(hit.position);
                    noPath = true;
                }
            } else if (!noPath)
            {
                agent.SetDestination(dest);
            }
        }

        if (distThing < agent.stoppingDistance && (realDist < attackDist || noPath))
        {
            noPath = false;
            transform.forward = Vector3.Slerp(transform.forward, player.transform.position - transform.position, 2 * Time.deltaTime);

            if (agent.isOnNavMesh && agent.isActiveAndEnabled) agent.isStopped = true;

            rb.isKinematic = false;
            animator.SetBool("isWalking", false);

            /*if (agent.isOnNavMesh && agent.isActiveAndEnabled) agent.isStopped = true;

            rb.isKinematic = false;
            animator.SetBool("isWalking", false);*/

            if (realDist < attackDist)
            {
                agent.enabled = false;
                obstacle.enabled = true;
                if (!attackCooldown && !weapon.attacking)
                {
                    attackCooldown = true;
                    Invoke(nameof(Attack), startupTime);
                }
            }
        } else
        {
            if (!isWaiting)
            {
                isWaiting = true;
                Invoke(nameof(ResetAgent), timeToResume);
            }
        }

    }

    //ADD TIME SINCE PATH AVALIABLE SO ONLY RELIABLE PATHS CAUSE THESE UPDATES (ONLY FOR PPL WITH NOPATH AS TRUE)

    private void ResetAgent()
    {
        if (health <= 0) return;
        if (weapon.attacking)
        {
            Invoke(nameof(ResetAgent), timeToResume);
            return;
        }
        if (distThing >= agent.stoppingDistance || Vector3.Distance(transform.position, player.transform.position) > attackDist && pathGuy.pathAvaliable && pathGuy.timeSinceAvaliable >= 1f)
        {
            noPath = false;
            obstacle.enabled = false;
            Invoke(nameof(ResetTwo), 0.2f);
            return;
        }
        isWaiting = false;
    }

    private void ResetTwo()
    {
        isWaiting = false;
        if (obstacle.enabled) return;
        agent.enabled = true;
        agent.isStopped = false;
        rb.isKinematic = true;
        animator.SetBool("isWalking", true);
        agent.SetDestination(dest);
        distThing = agent.remainingDistance;
    }

    private void ResetHitCooldown()
    {
        hitCooldown = false;
    }

    public void TargetedToggle()
    {
        //Debug.Log(!targeted);
        targeted = !targeted;
        if (targeted)
        {
            visualOption.GetComponent<SkinnedMeshRenderer>().materials = new Material[2] { origMat, targetedMat };
            //visualOption.GetComponent<SkinnedMeshRenderer>().SetMaterials(new List<Material> { origMat, targetedMat });
        } else
        {
            visualOption.GetComponent<SkinnedMeshRenderer>().materials = new Material[1] { origMat };
            //visualOption.GetComponent<SkinnedMeshRenderer>().SetMaterials(new List<Material> { origMat });
            //visualOption.GetComponent<SkinnedMeshRenderer>().
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (health <= 0) return;
        //Debug.Log(collision.gameObject.name);
        if (collision.gameObject.tag == "Weapon")
        {
            Instantiate(bloodEffect, collision.GetContact(0).point, Quaternion.LookRotation(-collision.GetContact(0).normal), transform);
            int mIndex = UnityEngine.Random.Range(0, hitSources.Count);
            hitSources.ElementAt(mIndex).Play();
            if (!hitCooldown)
            {
                health -= player.GetComponent<Weapon>().damage;
                healthSlider.value = Mathf.Max(0, health);
                if (health <= 0)
                {
                    healthSlider.transform.parent.gameObject.SetActive(false);
                    animator.SetTrigger("isDead");
                    gameObject.layer = 0;
                    if (targeted)
                    {
                        player.GetComponent<Player>().UpdateTargets(false, true);
                    }
                    rb.isKinematic = false;
                    weapon.mesh.enabled = false;
                    GetComponent<CapsuleCollider>().excludeLayers = LayerMask.GetMask("Enemy", "Player");
                    OnDeath?.Invoke(gameObject, scoreOnDeath);
                    return;
                }
                hitCooldown = true;
                agent.enabled = false;
                rb.isKinematic = false;
                animator.SetTrigger("isHit");
                Invoke(nameof(ResetHitCooldown), knockbackTime);
                if (currentAttack != null && weapon.attackPoise <= collision.gameObject.GetComponent<WeaponParent>().hParent.GetComponent<Weapon>().attackPoise + 1)
                {
                    weapon.AttackEnd();
                    rb.AddForce(new Vector3(transform.position.x - collision.transform.position.x, 0.5f, transform.position.z - collision.transform.position.z).normalized * knockback, ForceMode.Impulse);
                } else
                {
                    rb.AddForce(new Vector3(transform.position.x - collision.transform.position.x, 0.5f, transform.position.z - collision.transform.position.z).normalized * knockback, ForceMode.Impulse);
                }
            }
        }
    }

    public void Die()
    {
        manager.GetComponent<EnemyManager>().enemyDied();
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        float realDist = Vector3.Distance(transform.position, player.transform.position);
        if (!agent.enabled && !weapon.attacking && realDist < 3.0f && !wasAttacked)
        {
            animator.SetBool("tooClose", true);
            transform.Translate(-transform.forward * Time.fixedDeltaTime * .5f, Space.World);
        } else
        {
            animator.SetBool("tooClose", false);
            Vector3 xzVel = new Vector3(agent.velocity.x, 0, agent.velocity.z);
            float xzMag = xzVel.magnitude;
            //Debug.Log(xzMag);
            animator.SetBool("isRunning", xzMag > 4 + 0.2f && realDist > attackDist);
        }
    }
}
