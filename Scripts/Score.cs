using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    public static float score;

    public static int level = 0;

    public static int potentialLevel = 0;

    public static bool canBuyLevels = false;

    //public delegate void ScoreCallback(float score);
    //public static event ScoreCallback ScoreUpdated;
    [SerializeField]
    private List<float> pointsToLevel;

    private float PointsToLevel(int lvl)
    {
        if (lvl < 0) return 0;
        try
        {
            return pointsToLevel[lvl];
        } catch
        {
            return 2 * lvl;
        }
    }

    public static event Action<float> OnScoreUpdated;

    //previous score needed, next score needed, new level
    public static event Action<float, float, bool> OnLevelUp;

    private void OnEnable()
    {
        Enemy.OnDeath += (GameObject _, float incr) => UpdateScore(incr);
    }

    private void OnDisable()
    {
        Enemy.OnDeath -= (GameObject _, float incr) => UpdateScore(incr);
    }

    public void UpdateScore(float increase)
    {
        score += increase;
        OnScoreUpdated?.Invoke(score);
        while (score >= PointsToLevel(potentialLevel)) potentialLevel++;
        if (potentialLevel > level)
        {
            OnLevelUp?.Invoke(PointsToLevel(potentialLevel - 1), PointsToLevel(potentialLevel), false);
            canBuyLevels = true;
        }
    }

    private void Start()
    {
        canBuyLevels = false;
        OnLevelUp?.Invoke(0, PointsToLevel(0), true);
    }
}
