using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUp : MonoBehaviour
{
    public static LevelUp instance { get; private set; }

    [SerializeField]
    private ScoreBar scoreBar;

    [SerializeField]
    private GameObject upgradePrefab;

    [SerializeField]
    private Transform[] upgradeSpawns;

    [SerializeField]
    private List<Upgrade> upgrades;

    [SerializeField]
    private float timeBeforeHidden;

    private Dictionary<Upgrade, int> upgradeLevels;

    public event Action HideCards;

    private System.Random _random = new System.Random();

    private bool closedByForce = false;

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < (n - 1); i++)
        {
            int r = i + _random.Next(n - i);
            T t = list[r];
            list[r] = list[i];
            list[i] = t;
        }
    }

    private void Awake()
    {
        instance = this;
        if (upgradeLevels == null) upgradeLevels = new Dictionary<Upgrade, int>();
    }

    private void ShowCards()
    {
        Shuffle(upgrades);
        for (int i = 0; i < Math.Min(upgradeSpawns.Length, upgrades.Count); i++)
        {
            Upgrade myUpgrade = upgrades[i];

            GameObject newUpgrade = Instantiate(upgradePrefab, upgradeSpawns[i]);

            upgradeLevels.TryGetValue(myUpgrade, out int level);

            newUpgrade.GetComponent<UpgradeButton>().Setup(level, myUpgrade);
        }
    }

    private void OnEnable()
    {
        if (!closedByForce)
        {
            ShowCards();
            closedByForce = true;
        }
    }

    public IEnumerator UpgradeSelected(Upgrade upgrade)
    {
        upgradeLevels.TryGetValue(upgrade, out int level);
        if (upgrade.maxLevel - 1 <= level)
        {
            upgrades.Remove(upgrade);
        } else
        {
            upgradeLevels[upgrade] = level + 1;
        }

        yield return new WaitForSecondsRealtime(timeBeforeHidden);

        Score.level++;
        if (Score.potentialLevel == Score.level)
        {
            Score.canBuyLevels = false;
            scoreBar.LeveledUp();
            HideCards?.Invoke();
            closedByForce = false;
            UI.Instance.toggleUpgrade();
        } else
        {
            HideCards?.Invoke();
            ShowCards();
        }
        Upgrade.hidden = false;
    }
}
