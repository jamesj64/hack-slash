using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class DeathEffect : MonoBehaviour
{
    private Volume volume;

    private ColorAdjustments colorAdjustments;

    [SerializeField]
    private float effectSpeed;

    private void Start()
    {
        volume = GetComponent<Volume>();

        volume.profile.TryGet(out colorAdjustments);
    }

    private void OnEnable()
    {
        Player.OnPlayerDied += StartEffect;
    }

    private void OnDisable()
    {
        Player.OnPlayerDied -= StartEffect;
    }

    private void StartEffect()
    {
        colorAdjustments.active = true;
        StartCoroutine(SaturationShift());
    }

    IEnumerator SaturationShift()
    {
        float sat = colorAdjustments.saturation.value;
        while (sat >= -100)
        {
            sat -= effectSpeed;
            colorAdjustments.saturation.value = Mathf.Max(-100, sat);
            yield return new WaitForEndOfFrame();
        }
    }
}
