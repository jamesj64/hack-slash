using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region Declarations

    public static Player player { get; private set; }

    private PlayerInput input;

    private InputAction movement;

    private Rigidbody rb;
    
    private Vector2 inputVec;

    [Header("Movement")]
    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private float rotSpeed;

    [SerializeField]
    public float maxSpeedWalk;

    [SerializeField]
    public float maxSpeedRun;

    [SerializeField]
    private float jumpForce;

    [SerializeField]
    private Transform camTransform;

    [Header("Ground Check")]
    [SerializeField]
    private float playerHeight;

    [SerializeField]
    private LayerMask ground;

    [SerializeField]
    private float groundDrag;

    [SerializeField]
    private float dampening;

    [SerializeField]
    private bool grounded = true;

    [SerializeField]
    private float jumpCooldown;

    private bool canJump = true;

    private Animator animator;

    [HideInInspector]
    public bool sprinting = false;

    [Header("Effects")]
    [SerializeField]
    private GameObject bloodEffect;

    private Volume volume;

    [SerializeField]
    private float effectDuration;

    private Vignette vignette;

    private ColorAdjustments colorAdjustments;

    private float timeSinceEffect = 0;

    [Header("Combat")]

    //[SerializeField]
    //private float health;

    //public static float maxHealth = 200;

    //public float stamina;

    //public static float maxStamina = 200;

    //public static float healthRegen = 0;

    //[SerializeField]
    //private float staminaRegenPS;

    [SerializeField]
    private float stunTime;

    [SerializeField]
    private LayerMask enemy;

    [SerializeField]
    private float lockOnBubble;

    [SerializeField]
    private float lockOnRange;

    [SerializeField]
    private GameObject freeAimCam;

    [SerializeField]
    private GameObject combatCam;

    [SerializeField]
    private Transform combatCamHelper;

    private int currentTargetIndex = 0;

    private SortedList<float, GameObject> targets;

    [HideInInspector]
    public GameObject lockOnTarget;

    [Header("Audio")]
    [SerializeField]
    private List<AudioSource> hitSources;

    private CinemachineTargetGroup targetGroup;

    [HideInInspector]
    public string currentAttack = null;

    private Weapon weapon;

    [HideInInspector]
    public bool isStunned;

    [Header("UI")]
    [SerializeField]
    private HealthBar healthSlider;

    //[SerializeField]
    //private StaminaBar staminaBar;

    private bool stunnedStam = false;

    //public static event Action<float, bool> OnStaminaUpdated;

    //public static event Action<float, bool> OnHealthUpdated;

    public static event Action OnPlayerDied;

    private PlayerStats stats;

    #endregion

    private void Awake()
    {
        player = this;
    }

    private void Start()
    {
        stats = PlayerStats.stats;

        //stamina = maxStamina;

        targets = new SortedList<float, GameObject>();

        camTransform = Camera.main.transform;

        weapon = GetComponent<Weapon>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        input = UI.Instance.input;

        movement = input.Player.Move;
        movement.Enable();

        input.Player.Jump.performed += DoJump;
        input.Player.Jump.Enable();

        input.Player.Sprint.performed += DoSprint;
        input.Player.Sprint.Enable();

        input.Player.Attack1.performed += (InputAction.CallbackContext context) => { DoAttack("gAttack1"); };
        input.Player.Attack1.Enable();

        input.Player.Attack2.performed += (InputAction.CallbackContext context) => { DoAttack("gAttack2"); };
        input.Player.Attack2.Enable();

        input.Player.Attack3.performed += (InputAction.CallbackContext context) => { DoAttack("gAttack3"); };
        input.Player.Attack3.Enable();

        input.Player.LockOn.performed += DoLockOn;
        input.Player.LockOn.Enable();

        input.Player.ShuffleTarget.performed += (_) => { shuffleTarget(); };
        input.Player.ShuffleTarget.Enable();

        targetGroup = combatCam.GetComponent<CinemachineVirtualCamera>().LookAt.GetComponent<CinemachineTargetGroup>();

        volume = GameObject.Find("Global Volume").GetComponent<Volume>();

        volume.profile.TryGet(out vignette);

        volume.profile.TryGet(out colorAdjustments);

        /*healthSlider.maxValue = maxHealth;
        health = maxHealth;
        healthSlider.value = health;*/

        /*staminaBar.maxValue = maxStamina;
        staminaBar.value = stamina;
        stamina = maxStamina;*/
    }

    /*public void smoothStam(float loss)
    {
        stamina = Math.Max(0, stamina - loss);
        //staminaBar.SmoothSet(stamina);
        OnStaminaUpdated?.Invoke(stamina, true);
    }*/

    private void ResetJump()
    {
        canJump = true;
    }

    private void ResetStun()
    {
        isStunned = false;
    }

    private void SetGrounded()
    {
        grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.3f, ground);
    }

    IEnumerator HitEffect()
    {
        if (vignette.active) yield break;
        timeSinceEffect = 0;
        colorAdjustments.active = true;
        vignette.active = true;
        yield return new WaitForSecondsRealtime(effectDuration);
        vignette.active = false;
        //if (health > 0) colorAdjustments.active = false;
        if (stats.hp > 0) colorAdjustments.active = false;
    }

    private void DoJump(InputAction.CallbackContext context)
    {
        if (grounded && canJump && !isStunned)
        {
            canJump = false;
            grounded = false;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            animator.SetBool("isJumping", true);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void DoLockOn(InputAction.CallbackContext context)
    {
        UpdateTargets(true, false);
    }

    private void DoSprint(InputAction.CallbackContext context)
    {
        sprinting = !stunnedStam && stats.stamina > 0 && context.ReadValue<float>() == 1;
    }

    private void DoAttack(string trigger)
    {
        if (isStunned || stats.stamina < 5) return;
        currentAttack = trigger;
        animator.SetTrigger(trigger);
    }

    public void UpdateTargets(bool changeView, bool dead)
    {

        if (lockOnTarget == null && dead) return;

        if (dead && lockOnTarget != null)
        {
            lockOnTarget.GetComponent<Enemy>().TargetedToggle();
            targetGroup.RemoveMember(lockOnTarget.transform);
            lockOnTarget = null;
            UpdateTargets(true, false);
            return;
        }

        if (lockOnTarget != null && changeView)
        {
            combatCam.SetActive(false);
            freeAimCam.SetActive(true);
            lockOnTarget.GetComponent<Enemy>().TargetedToggle();
            lockOnTarget = null;
            targets = new SortedList<float, GameObject>();
            foreach(CinemachineTargetGroup.Target target in targetGroup.m_Targets) {
                if (transform != target.target.transform) targetGroup.RemoveMember(target.target.transform);
            }
            return;
        }

        if (!changeView && lockOnTarget == null) return;

        if (changeView)
        {
            foreach (CinemachineTargetGroup.Target target in targetGroup.m_Targets)
            {
                if (transform != target.target.transform) targetGroup.RemoveMember(target.target.transform);
            }

        }

        int numTargets = 0;

        if (changeView) lockOnTarget = null;
        targets = new SortedList<float, GameObject>();

        float closestDist = Mathf.Infinity;

        Collider[] colliders = Physics.OverlapSphere(transform.position, lockOnRange, enemy);

        foreach (Collider collider in colliders)
        {
            float angleFromCam = Vector3.Angle(camTransform.forward, collider.transform.position - transform.position);
            float dist = Vector3.Distance(transform.position, collider.transform.position);
            if (Math.Abs(angleFromCam) < 75 || dist < lockOnBubble)
            {
                targets.Add(dist, collider.gameObject);
                if (closestDist >= dist)
                {
                    closestDist = dist;
                    if (changeView) lockOnTarget = collider.gameObject;
                }
                numTargets++;
            }
        }


        if (lockOnTarget == null && changeView)
        {
            combatCam.SetActive(false);
            freeAimCam.SetActive(true);
        }

        if (lockOnTarget != null && changeView)
        {
            targetGroup.AddMember(lockOnTarget.transform, 1, 0);
            combatCam.SetActive(true);
            freeAimCam.SetActive(false);
            lockOnTarget.GetComponent<Enemy>().TargetedToggle();
        }

    }

    private void shuffleTarget()
    {
        UpdateTargets(false, false);
        if (targets.Count < 2) return;
        currentTargetIndex = (currentTargetIndex + 1) % targets.Count;
        if (targets[targets.Keys[currentTargetIndex]] == lockOnTarget)
        {
            currentTargetIndex = (currentTargetIndex + 1) % targets.Count;
        }
        if (lockOnTarget != null) 
        { 
            lockOnTarget.GetComponent<Enemy>().TargetedToggle();
            targetGroup.RemoveMember(lockOnTarget.transform);
        }
        lockOnTarget = targets[targets.Keys[currentTargetIndex]];
        lockOnTarget.GetComponent<Enemy>().TargetedToggle();
        targetGroup.AddMember(lockOnTarget.transform, 1, 0);

    }

    private void Update()
    {
        /*if (stamina <= 0 && !weapon.attacking)
        {
            StartCoroutine(StamStun(3f));
        }*/

        /*if (healthRegen != 0 && !isStunned && health > 0)
        {
            health = Mathf.Min(health + healthRegen * Time.deltaTime, maxHealth);
            OnHealthUpdated?.Invoke(health, false);
        }*/

        stats.staminaDraining = sprinting && inputVec != Vector2.zero;

        stats.staminaRegenerating = !weapon.attacking && !isStunned && !stunnedStam;

        /*if (sprinting && inputVec != Vector2.zero)
        {
            stamina = Mathf.Max(0, stamina - Time.deltaTime * staminaRegenPS * 0.25f);
            OnStaminaUpdated?.Invoke(stamina, false);
            if (stamina <= 0)
            {
                sprinting = false;
            }
        } else if (!weapon.attacking && !isStunned && !stunnedStam)
        {
            stamina = Mathf.Min(stamina + staminaRegenPS * Time.deltaTime, maxStamina);
            //staminaBar.value = stamina;
            OnStaminaUpdated?.Invoke(stamina, false);
        }*/

        /*if (!isStunned && health > 0)
        {
            health = Mathf.Min(maxHealth, health + Time.deltaTime);
            healthSlider.value = health;
        }*/

        if (vignette && vignette.active) {
            float pctComplete = Math.Min(timeSinceEffect / effectDuration, 1);
            vignette.intensity.value = 0.65f + 0.1f * Mathf.Sin(Mathf.PI * pctComplete);
            Color c = colorAdjustments.colorFilter.value;
            float newGB = 0.45f + 0.55f * pctComplete;
            colorAdjustments.colorFilter.value = new Color(c.r, newGB, newGB, c.a);
            timeSinceEffect += Time.deltaTime;
        }

        inputVec = isStunned ? Vector2.zero : movement.ReadValue<Vector2>();
        animator.SetBool("isWalking", Vector2.zero != inputVec);

        Vector3 inputDir;

        if (lockOnTarget != null)
        {
            Vector3 displacement = lockOnTarget.transform.position - new Vector3(transform.position.x, lockOnTarget.transform.position.y, transform.position.z);
            inputDir = displacement * (0.02f + inputVec.y) - Vector3.Cross(displacement, Vector3.up) * inputVec.x;
            combatCamHelper.position = transform.position - displacement.normalized * 3;
            if (Vector3.Distance(lockOnTarget.transform.position, transform.position) > lockOnRange) UpdateTargets(true, false);
        } else
        {
            Vector3 displacement = lockOnTarget == null ? transform.position - new Vector3(camTransform.position.x, transform.position.y, camTransform.position.z) : lockOnTarget.transform.position - new Vector3(transform.position.x, lockOnTarget.transform.position.y, transform.position.z);
            inputDir = displacement * inputVec.y - Vector3.Cross(displacement, Vector3.up) * inputVec.x;
        }

        if (inputDir != Vector3.zero) transform.forward = Vector3.Slerp(transform.forward, inputDir.normalized, Time.deltaTime * rotSpeed);
    }

    public IEnumerator StamStun(float duration)
    {
        if (stunnedStam) yield break;
        sprinting = false;
        float initSpeed = maxSpeedWalk;
        stunnedStam = true;
        maxSpeedWalk *= 0.45f;
        animator.SetFloat("walkSpeed", 0.8f);
        yield return new WaitForSeconds(duration);
        if (stats.stamina == 0) stats.SetStamina(0.1f, false);
        maxSpeedWalk = initSpeed;
        stunnedStam = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            WeaponParent wp = other.gameObject.GetComponent<WeaponParent>();
            if (wp == null || wp.onCooldown) return;
            Weapon otherWeapon = wp.hParent.GetComponent<Weapon>();
            //health -= otherWeapon.damage;
            stats.SetHp(stats.hp - otherWeapon.damage, true);
            //healthSlider.value = Mathf.Max(0, health);
            //OnHealthUpdated?.Invoke(health, true);
            wp.onCooldown = true;
            Vector3 bloodPos = other.ClosestPointOnBounds(transform.position + Vector3.up * playerHeight / 2);
            Vector3 disp = new Vector3(bloodPos.x - transform.position.x, 0, bloodPos.z - transform.position.z);
            Instantiate(bloodEffect, bloodPos, disp == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(disp), transform);
            StartCoroutine(HitEffect());
            int mIndex = UnityEngine.Random.Range(0, hitSources.Count);
            hitSources.ElementAt(mIndex).Play();
            if (stats.hp <= 0)
            {
                return;
            }
            if (currentAttack != null)
            {
                if (weapon.attackPoise <= otherWeapon.attackPoise + 1)
                {
                    isStunned = true;
                    weapon.AttackEnd();
                    animator.SetTrigger("isHit");
                    Invoke(nameof(ResetStun), stunTime);
                    StartCoroutine(StamStun(0.75f));
                }
            } else
            {
                isStunned = true;
                StartCoroutine(StamStun(0.45f));
                animator.SetTrigger("isHit");
                weapon.AttackEnd();
                Invoke(nameof(ResetStun), stunTime);
            }
        }
    }

    public void Dead()
    {
        input.Disable();
        if (lockOnTarget != null) UpdateTargets(true, false);
        animator.SetTrigger("isDead");
        OnPlayerDied();
        rb.isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
        Time.timeScale = 0.75f;
    }

    public void Die()
    {
        StartCoroutine(DeathAnim());
    }

    IEnumerator DeathAnim()
    {
        yield return new WaitForSecondsRealtime(2);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (((1<<collision.gameObject.layer) & ground) != 0)
        {
            animator.SetBool("isJumping", false);
            //Debug.Log(animator.GetBool("isJumping"));
            SetGrounded();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.3f, ground);
        if (collision == null) return;
        if (collision.gameObject.layer == ground)
        {
            Invoke(nameof(SetGrounded), 0.25f);
        }
    }
    

    private void FixedUpdate()
    {
        //grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.3f, ground);

        rb.drag = grounded ? groundDrag : 0f;

        Vector3 moveDir = (transform.forward * Math.Max(Math.Abs(inputVec.y), Math.Abs(inputVec.x))).normalized;
        Vector3 xzVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float xzMag = xzVel.magnitude;

        if (!stunnedStam) animator.SetFloat("walkSpeed", xzMag > maxSpeedWalk / 2 ? 1.35f : 1.15f);

        animator.SetBool("isRunning", xzMag > maxSpeedWalk + 0.2f);

        //Debug.Log(xzMag);

        float speedDif = (sprinting ? maxSpeedRun : maxSpeedWalk) - xzMag;
        if (speedDif < 0)
        {
            Vector3 targetVel = rb.velocity + speedDif * xzVel.normalized;
            Vector3.SlerpUnclamped(targetVel, rb.velocity, Time.fixedDeltaTime * dampening);

            if ((xzVel + moveDir).magnitude < xzMag) rb.AddForce(moveDir * moveSpeed * (lockOnTarget == null ? 1 : 0.45f), ForceMode.Force);

        } else
        {
            rb.AddForce(moveDir * moveSpeed * (lockOnTarget == null ? 1 : 0.45f), ForceMode.Force);
        }
    }
}
