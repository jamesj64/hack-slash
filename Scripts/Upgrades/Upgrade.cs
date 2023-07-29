using UnityEngine;


[CreateAssetMenu(fileName = "Upgrade", menuName = "ScriptableObjects/Upgrades", order = 1)]
public class Upgrade : ScriptableObject
{

    public static bool hidden = false;

    public string upgradeName;

    public string description;

    public enum AttributeType
    {
        Health,
        Stamina,
        Knockback,
        WalkSpeed,
        HealthRegen,
        StaminaRegen,
        AttackSpeed
    }

    public AttributeType attributeType;

    public float[] effectValues;

    public int maxLevel;

    public virtual void ApplyUpgrade(int level)
    {
        if (!hidden)
        {
            hidden = true;
            maxLevel = effectValues.Length;
            if (maxLevel <= level) return;
            switch (attributeType)
            {
                case AttributeType.Health:
                    PlayerStats.stats.maxHp += effectValues[level];
                    PlayerStats.stats.SetHp(PlayerStats.stats.maxHp, true);
                    Debug.Log("Max Health: " + PlayerStats.stats.maxHp + "; level: " + level + "; maxLvl: " + maxLevel);
                    break;
                case AttributeType.Stamina:
                    PlayerStats.stats.maxStamina += effectValues[level];
                    PlayerStats.stats.SetStamina(PlayerStats.stats.maxStamina, false);
                    Debug.Log("Max Stamina " + PlayerStats.stats.maxStamina + "; level: " + level + "; maxLvl: " + maxLevel);
                    break;
                case AttributeType.HealthRegen:
                    PlayerStats.stats.hpRegen += effectValues[level];
                    Debug.Log("Health Regen " + PlayerStats.stats.hpRegen + "; level: " + level + "; maxLvl: " + maxLevel);
                    break;
                case AttributeType.StaminaRegen:
                    PlayerStats.stats.staminaRegen += effectValues[level];
                    Debug.Log("Health Regen " + PlayerStats.stats.staminaRegen + "; level: " + level + "; maxLvl: " + maxLevel);
                    break;

            } 
        }
    }
}
