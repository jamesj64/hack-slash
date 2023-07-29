using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothBar : StatusBar
{

    private float destVal;

    [SerializeField]
    private float vel;

    private float effVel;

    private float negMultiplier;

    private bool isSmoothing = false;

    public void SmoothSet(float val)
    {
        if (maxValue == minValue)
        {
            value = minValue;
            return;
        }

        if (val == value)
        {
            return;
        }

        negMultiplier = val > value ? 1 : -1;

        effVel = negMultiplier * vel * (maxValue - minValue);

        isSmoothing = true;

        destVal = val;
    }

    public override float value
    {
        get { return base.value; }
        set
        {
            isSmoothing = false;
            base.value = value;
        }
    }

    protected virtual void HandleOverflow() { }

    private void Update()
    {
        if (!isSmoothing) return;
        if ((value <= destVal && negMultiplier == 1) || (value >= destVal && negMultiplier == -1))
        {
            float incrVal = value + Time.deltaTime * effVel;
            if (incrVal >= maxValue && negMultiplier == 1)
            {
                base.value = maxValue;
                HandleOverflow();
                isSmoothing = false;
            } else if (incrVal <= minValue && negMultiplier == -1)
            {
                base.value = minValue;
                HandleOverflow();
            } else
            {
                base.value = incrVal;
            }
        } else
        {
            value = destVal;
            isSmoothing = false;
        }
    }
}
