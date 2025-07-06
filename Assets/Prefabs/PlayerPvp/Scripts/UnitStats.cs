using UnityEngine;

[System.Serializable]
public class StatGrowth
{
    public int HPPerLevel = 2;
    public int AttackPerLevel = 2;
    public int SpeedPerLevel = 3;
    public float CritChancePerLevel = 2f;      // +2% per level
    public float CritDamagePerLevel = 0.05f;   // +5% per level
    public float DodgeChancePerLevel = 0f;
}

public class UnitStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int baseHP = 10;
    public int baseAttack = 2;
    public int baseSpeed = 1;
    [Range(0, 100)] public float baseCritChance = 10f;
    [Range(0, 10f)] public float critMultiplier = 1.5f;
    [Range(0, 100)] public float baseDodgeChance = 0f;

    [Header("Growth Per Level")]
    public StatGrowth growth;

    [Header("Level")]
    public int currentLevel = 1;
    public int maxLevel = 20;

    public UnitStatsData GetStats()
    {
        return new UnitStatsData
        {
            HP = baseHP + growth.HPPerLevel * (currentLevel - 1),
            Attack = baseAttack + growth.AttackPerLevel * (currentLevel - 1),
            Speed = baseSpeed + growth.SpeedPerLevel * (currentLevel - 1),
            CritChance = baseCritChance + growth.CritChancePerLevel * (currentLevel - 1),
            CritMultiplier = critMultiplier + growth.CritDamagePerLevel * (currentLevel - 1),
            DodgeChance = baseDodgeChance + growth.DodgeChancePerLevel * (currentLevel - 1)
        };
    }

    [ContextMenu("Level Up")]
    public void LevelUp()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            Debug.Log($"{name} leveled up to {currentLevel}");

            // Optionally, notify CombatHandler to update UI
            // UnitCombatHandler combatHandler = GetComponent<UnitCombatHandler>();
            // if (combatHandler != null)
            // {
            //     combatHandler.UpdateHealthText();
            //     combatHandler.UpdateSpeedText();
            // }
        }
        else
        {
            Debug.Log($"{name} is already at max level!");
        }
    }
}

public struct UnitStatsData
{
    public int HP;
    public int Attack;
    public int Speed;
    public float CritChance;
    public float CritMultiplier;
    public float DodgeChance;
}
