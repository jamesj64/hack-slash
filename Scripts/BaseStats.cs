using UnityEngine;

public abstract class BaseStats : ScriptableObject
{
    [field: Header("Health")]

    [field: SerializeField]
    public float hp { get; private set; }
    [field: SerializeField]
    public float maxHp { get; private set; }
    public float minHp { get; private set; }
    [field: SerializeField]
    public float hpRegen { get; private set; }
}
