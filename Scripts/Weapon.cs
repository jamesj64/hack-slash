using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public MeshCollider mesh;

    [SerializeField]
    private GameObject weapon;

    [SerializeField]
    private float attackSpeedMultiplier;

    private Player controller = null;

    private bool isEnemy;

    public float damage;

    [HideInInspector]
    public float initMaxWalk;
    [HideInInspector]
    public float initMaxRun;

    public bool attacking = false;

    private WeaponParent wp;

    private Transform player;

    public int attackPoise = 0;

    // Start is called before the first frame update
    private void Start()
    {
        mesh = weapon.GetComponent<MeshCollider>();
        wp = weapon.GetComponent<WeaponParent>();
        isEnemy = tag != "Player";
        if (isEnemy)
        {
            mesh.enabled = false;
            player = GameObject.FindGameObjectWithTag("Player").transform;
        } else
        {
            mesh.isTrigger = true;
            controller = GetComponent<Player>();
            initMaxWalk = controller.maxSpeedWalk;
            initMaxRun = controller.maxSpeedRun;
        }
    }

    public void AttackStart(AnimationEvent evt)
    {
        damage = evt.floatParameter;
        attackPoise = evt.intParameter;
        wp.onCooldown = false;
        if (isEnemy)
        {
            mesh.enabled = true;
            //wp.onCooldown = false;
        }
        else
        {
            mesh.isTrigger = false;
            controller.maxSpeedRun = initMaxRun * attackSpeedMultiplier;
            controller.maxSpeedWalk = initMaxWalk * attackSpeedMultiplier;
        }
    }

    public void attackInit()
    {
        attacking = true;
    }

    public void UpdateDamage(AnimationEvent evt)
    {
        float dam = evt.floatParameter;
        attackPoise = evt.intParameter;
        wp.onCooldown = dam == 0;
        damage = dam;
    }

    public void StaminaCost(float cost)
    {
        if (!isEnemy)
        {
            PlayerStats.stats.IncrementStamina(-cost, true);
        }
    }

    public void AttackEnd()
    {
        attacking = false;
        attackPoise = 0;
        wp.onCooldown = true;
        if (isEnemy)
        {
            //wp.onCooldown = false;
            mesh.enabled = false;
            //mesh.isTrigger = false;
        }
        else
        {
            mesh.isTrigger = true;
            controller.currentAttack = null;
            controller.maxSpeedRun = initMaxRun;
            controller.maxSpeedWalk = initMaxWalk;
        }
    }

    private void FixedUpdate()
    {
        if (attacking)
        {
            if (isEnemy && player)
            {
                Vector3 otherPos = player.position;
                if (Vector3.Distance(otherPos, transform.position) >= 2f)
                {
                    transform.Translate((otherPos - transform.position).normalized * Time.fixedDeltaTime * 2f, Space.World);
                }
            } else if (controller != null && controller.lockOnTarget != null)
            {
                Vector3 otherPos = controller.lockOnTarget.transform.position;
                if (Vector3.Distance(otherPos, transform.position) >= 2)
                    transform.Translate((otherPos - transform.position).normalized * Time.fixedDeltaTime, Space.World);
            }
        }
    }
}
