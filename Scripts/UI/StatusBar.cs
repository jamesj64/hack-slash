using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class StatusBar : MonoBehaviour
{
    public float minValue;

    public float maxValue;
    
    [SerializeField]
    private float _value;
    
    [SerializeField]
    private RectTransform fill;

    protected RawImage image;

    protected virtual void Start()
    {
        image = GetComponent<RawImage>();
        fill.localScale = new Vector3(normalize(_value), 1, 1);
    }

    public virtual float value 
    { 
        get { return _value; }
        set 
        {
            _value = Mathf.Clamp(value, minValue, maxValue);
            fill.localScale = new Vector3(normalize(_value), 1, 1);
        } 
    }

    protected float normalize(float val)
    {
        if (maxValue == minValue) return 1;
        return (val - minValue) / (maxValue - minValue);
    }

    protected void UpdateConstraints(float val, float min, float max)
    {
        minValue = min;
        maxValue = max;
        value = val;
    }

}
