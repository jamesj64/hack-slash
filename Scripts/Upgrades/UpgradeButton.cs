using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeButton : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI buttonText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    private int level;

    private Upgrade upgrade;

    public void Setup(int lvl, Upgrade up)
    {
        level = lvl;
        upgrade = up;
        buttonText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
    }

    public void OnUpgrade()
    {
        upgrade.ApplyUpgrade(level);
        LevelUp.instance.StartCoroutine(LevelUp.instance.UpgradeSelected(upgrade));
    }

    private void DestroyMe() { Destroy(gameObject); }

    private void OnEnable()
    {
        LevelUp.instance.HideCards += DestroyMe;
    }

    private void OnDisable()
    {
        LevelUp.instance.HideCards -= DestroyMe;
    }
}
