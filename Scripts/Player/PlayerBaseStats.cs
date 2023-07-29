using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseStats", menuName = "ScriptableObjects/PlayerBaseStats", order = 1)]
public class PlayerBaseStats : BaseStats
{
    [field: SerializeField]
    public float stamina { get; private set; } = 1;
    [field: SerializeField]
    public float maxStamina { get; private set; }
    public float minStamina { get; private set; }
    [field: SerializeField]
    public float staminaRegen { get; private set; }
}
