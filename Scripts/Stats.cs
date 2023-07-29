using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Stats : MonoBehaviour
{
    [field: Header("Health")]

    [SerializeField]
    private float _hp;

    public float hp 
    { 
        get { return _hp; } 
        set { _hp = Mathf.Clamp(value, minHp, maxHp); } 
    }
    [field: SerializeField]
    public virtual float maxHp { get; set; }
    public float minHp { get; private set; } = 0f;
    [field: SerializeField]
    public float hpRegen { get; set; }

    private float baseHp, baseMaxHp, baseMinHp, baseHpRegen;

    protected virtual void Awake()
    {
        baseHp = hp;
        baseMaxHp = maxHp;
        baseMinHp = minHp;
        baseHpRegen = hpRegen;
    }
}
