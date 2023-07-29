using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaBar : SmoothBar
{
    [SerializeField]
    private GameObject staminaText;

    private bool outOfStamina = false;

    protected override void Start()
    {
        base.Start();
        staminaText.SetActive(false);
    }

    private void UpdateStam(float stamina, bool smooth)
    {
        if (outOfStamina && stamina > 0)
        {
            outOfStamina = false;
            staminaText.SetActive(false);
        } 

        if (smooth)
        {
            SmoothSet(stamina);
        }
        else
        {
            value = stamina;
            if (stamina <= 0)
            {
                HandleOverflow();
            }
        }
    }

    private void OnEnable()
    {
        PlayerStats.OnStaminaUpdated += UpdateStam;
        PlayerStats.OnStaminaConstraintsUpdated += UpdateConstraints;
    }

    private void OnDisable()
    {
        PlayerStats.OnStaminaUpdated -= UpdateStam;
        PlayerStats.OnStaminaConstraintsUpdated -= UpdateConstraints;
    }

    protected override void HandleOverflow()
    {
        if (!outOfStamina)
        {
            outOfStamina = true;
            staminaText.SetActive(true);
        }
    }
}
