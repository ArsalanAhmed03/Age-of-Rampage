using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public List<UnitCombatHandler> playerFrontline;
    public List<UnitCombatHandler> playerBackline;
    public List<UnitCombatHandler> enemyFrontline;
    public List<UnitCombatHandler> enemyBackline;

    private int currentTurnIndex = 0;
    private List<UnitCombatHandler> turnQueue = new List<UnitCombatHandler>();

    private void Start()
    {
        turnQueue.AddRange(playerFrontline);
        turnQueue.AddRange(playerBackline);
        turnQueue.AddRange(enemyFrontline);
        turnQueue.AddRange(enemyBackline);

        turnQueue.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        NextTurn();
    }

    public void NextTurn()
    {
        if (turnQueue.Count == 0) return;

        UnitCombatHandler unit = turnQueue[currentTurnIndex];
        currentTurnIndex = (currentTurnIndex + 1) % turnQueue.Count;

        unit.StartTurn();
    }

    public UnitCombatHandler PickTarget(UnitCombatHandler attacker)
    {
        List<UnitCombatHandler> enemiesFront;
        List<UnitCombatHandler> enemiesBack;

        // Determine which team is the enemy
        if (playerFrontline.Contains(attacker) || playerBackline.Contains(attacker))
        {
            enemiesFront = enemyFrontline;
            enemiesBack = enemyBackline;
        }
        else
        {
            enemiesFront = playerFrontline;
            enemiesBack = playerBackline;
        }

        List<UnitCombatHandler> validFront = enemiesFront.FindAll(u => u.HP > 0);
        List<UnitCombatHandler> validBack = enemiesBack.FindAll(u => u.HP > 0);

        if (validFront.Count > 0)
            return validFront[Random.Range(0, validFront.Count)];
        if (validBack.Count > 0)
            return validBack[Random.Range(0, validBack.Count)];

        return null;
    }

    public void RemoveUnit(UnitCombatHandler unit)
    {
        turnQueue.Remove(unit);
        playerFrontline.Remove(unit);
        playerBackline.Remove(unit);
        enemyFrontline.Remove(unit);
        enemyBackline.Remove(unit);

        // Check for victory condition
        if (playerFrontline.Count + playerBackline.Count == 0)
        {
            Debug.Log("Enemy team wins!");
            return;
        }
        if (enemyFrontline.Count + enemyBackline.Count == 0)
        {
            Debug.Log("Player team wins!");
            return;
        }

        if (currentTurnIndex >= turnQueue.Count)
            currentTurnIndex = 0;

        // NextTurn();
    }
}
