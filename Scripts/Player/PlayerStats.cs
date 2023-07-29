using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerStats : Stats
{
    public static PlayerStats stats { get; private set; }

    [field: Header("Stamina")]

    [SerializeField]
    private float _stamina;
    public float stamina
    {
        get { return _stamina; }
        set { _stamina = Mathf.Clamp(value, minStamina, maxStamina); }
    }

    [SerializeField]
    private float _maxStamina;
    public float maxStamina
    {
        get { return _maxStamina; }
        set
        {
            _maxStamina = value;
            OnStaminaConstraintsUpdated?.Invoke(stamina, minStamina, maxStamina);
        }
    }

    public float minStamina { get; private set; }

    [field: SerializeField]
    public float staminaRegen { get; set; }

    [field: SerializeField]
    public float staminaRunCost { get; private set; }

    [HideInInspector]
    public bool staminaDraining;

    [HideInInspector]
    public bool staminaRegenerating;

    //private float baseStamina, baseMaxStamina, baseMinStamina, baseStaminaRegen;

    public static event Action<float, bool> OnHpUpdated;

    public static event Action<float, float, float> OnHpConstraintsUpdated;

    public static event Action<float, bool> OnStaminaUpdated;

    public static event Action<float, float, float> OnStaminaConstraintsUpdated;

    private Player player;

    public override float maxHp
    {
        get { return base.maxHp; }
        set 
        { 
            base.maxHp = value;
            OnHpConstraintsUpdated?.Invoke(hp, minHp, maxHp);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        stats = this;
        /*baseStamina = stamina;
        baseMaxStamina = maxStamina;
        baseMinStamina = minStamina;
        baseStaminaRegen = staminaRegen;*/
    }

    private void Start()
    {
        player = Player.player;
        OnHpConstraintsUpdated?.Invoke(hp, minHp, maxHp);
        OnStaminaConstraintsUpdated?.Invoke(stamina, minStamina, maxStamina);
    }

    private void OnEnable()
    {
        OnHpUpdated += UpdateHp;
        OnStaminaUpdated += UpdateStamina;
    }

    private void OnDisable()
    {
        OnHpUpdated -= UpdateHp;
        OnStaminaUpdated -= UpdateStamina;
    }

    private void Update()
    {
        if (hpRegen != 0 && hp > 0 && !player.isStunned)
        {
            OnHpUpdated?.Invoke(hp + hpRegen * Time.deltaTime, false);
        } else if (hp <= 0)
        {
            return;
        }

        if (staminaDraining)
        {
            OnStaminaUpdated?.Invoke(stamina - staminaRunCost * Time.deltaTime, false);
        } else if (staminaRegenerating && stamina != maxStamina)
        {
            OnStaminaUpdated?.Invoke(stamina + staminaRegen * Time.deltaTime, false);
        }
        if (stamina <= 0 && !GetComponent<Weapon>().attacking)
        {
            player.StartCoroutine(player.StamStun(3f));
        }
    }

    private void UpdateHp(float newHp, bool _)
    {
        hp = newHp;
        if (hp <= 0)
        {
            player.Dead();
        }
    }

    private void UpdateStamina(float newStamina, bool _)
    {
        stamina = newStamina;
    }

    public void IncrementStamina(float stamIncrease, bool smooth)
    {
        OnStaminaUpdated?.Invoke(stamina + stamIncrease, smooth);
    }

    public void SetHp(float newHp, bool smooth)
    {
        OnHpUpdated?.Invoke(newHp, smooth);
    }

    public void SetStamina(float newStamina, bool smooth)
    {
        OnStaminaUpdated?.Invoke(newStamina, smooth);
    }
}
