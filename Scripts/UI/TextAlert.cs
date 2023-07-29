using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(RectTransform))]
public class TextAlert : MonoBehaviour
{

    private RectTransform rectTransform;

    [SerializeField]
    private float scaleSize;

    [SerializeField]
    private float speed;

    private float negMod = 1;

    private float currentEffect;

    [SerializeField]
    private TextMeshProUGUI textMeshPro;

    [SerializeField]
    private bool isDeathMessage;

    [SerializeField]
    private bool outsourcedEnable;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void ToggleText(bool menuOpen)
    {
        if (isDeathMessage && !menuOpen) return;
        textMeshPro.enabled = !menuOpen;
    }

    private void ToggleDeath()
    {
        textMeshPro.enabled = isDeathMessage;
    }

    private void OnEnable()
    {
        if (outsourcedEnable) textMeshPro.enabled = true;
        UI.OnMenuToggle += ToggleText;
        Player.OnPlayerDied += ToggleDeath;
    }

    private void OnDisable()
    {
        UI.OnMenuToggle -= ToggleText;
        Player.OnPlayerDied -= ToggleDeath;
    }

    void Update()
    {
        if (textMeshPro != null && textMeshPro.enabled)
        {
            float incr = Time.deltaTime * speed * negMod;

            currentEffect += incr;

            rectTransform.localScale += new Vector3(incr, incr, 0);

            if (currentEffect >= scaleSize - 1)
            {
                negMod = -1;
            }
            else if (currentEffect < 0)
            {
                negMod = 1;
            }
        }
    }
}
