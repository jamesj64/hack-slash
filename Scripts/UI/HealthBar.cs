public class HealthBar : SmoothBar
{
    private void UpdateHealth(float health, bool smooth)
    {
        if (smooth)
        {
            SmoothSet(health);
        }
        else
        {
            value = health;
        }
    }

    private void OnEnable()
    {
        //Player.OnHealthUpdated += UpdateHealth;
        PlayerStats.OnHpUpdated += UpdateHealth;
        PlayerStats.OnHpConstraintsUpdated += UpdateConstraints;
    }

    private void OnDisable()
    {
        //Player.OnHealthUpdated -= UpdateHealth;
        PlayerStats.OnHpUpdated -= UpdateHealth;
        PlayerStats.OnHpConstraintsUpdated -= UpdateConstraints;
    }
}
