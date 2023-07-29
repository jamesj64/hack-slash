using System.Collections;
using UnityEngine;

public class ScoreBar : SmoothBar
{
    private float tgtMin;
    private float tgtMax;

    [SerializeField]
    private GameObject upgradeText;

    private bool hasUpdated = false;

    protected override void Start()
    {
        base.Start();
        upgradeText.SetActive(false);
        hasUpdated = false;
    }

    private void ScoreUpdated(float score)
    {
        if (!Score.canBuyLevels || !hasUpdated)
        {
            hasUpdated = true;
            SmoothSet(score);
        }
    }

    private void OnEnable()
    {
        Score.OnScoreUpdated += ScoreUpdated;
        Score.OnLevelUp += (float min, float max, bool isStart) => { if (isStart) maxValue = max; tgtMin = min; tgtMax = max; };
    }

    private void OnDisable() {
        Score.OnScoreUpdated -= ScoreUpdated;
        Score.OnLevelUp -= (float min, float max, bool isStart) => { if (isStart) maxValue = max; tgtMin = min; tgtMax = max; };
    }

    protected override void HandleOverflow()
    {
        upgradeText.SetActive(true);
    }

    public void LeveledUp()
    {
        minValue = tgtMin;
        maxValue = tgtMax;
        value = tgtMin;
        upgradeText.SetActive(Score.score == maxValue);
        SmoothSet(Score.score);
    }
}
