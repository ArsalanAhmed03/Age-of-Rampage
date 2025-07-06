using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    [Header("Spawn Parents")]
    public Transform playerFrontSpawnParent;
    public Transform playerBackSpawnParent;
    public Transform enemyFrontSpawnParent;
    public Transform enemyBackSpawnParent;

    private Transform[] playerFrontSpawns;
    private Transform[] playerBackSpawns;
    private Transform[] enemyFrontSpawns;
    private Transform[] enemyBackSpawns;

    private List<UnitCombatHandler> playerFrontline = new List<UnitCombatHandler>();
    private List<UnitCombatHandler> playerBackline = new List<UnitCombatHandler>();
    private List<UnitCombatHandler> enemyFrontline = new List<UnitCombatHandler>();
    private List<UnitCombatHandler> enemyBackline = new List<UnitCombatHandler>();

    public List<GameObject> enemyFrontPrefabs;
    public List<GameObject> enemyBackPrefabs;

    private int currentTurnIndex = 0;
    private List<UnitCombatHandler> turnQueue = new List<UnitCombatHandler>();

    private void Awake()
    {
        Instance = this;
    }

    public void InitializeBattle()
    {
        playerFrontSpawns = GetChildren(playerFrontSpawnParent);
        playerBackSpawns = GetChildren(playerBackSpawnParent);
        enemyFrontSpawns = GetChildren(enemyFrontSpawnParent);
        enemyBackSpawns = GetChildren(enemyBackSpawnParent);

        // Spawn Player Frontline
        for (int i = 0; i < 3; i++)
        {
            GameObject unitPrefab = LoadoutData.selectedUnits[i];
            if (unitPrefab == null) continue;

            GameObject go = Instantiate(unitPrefab, playerFrontSpawns[i].position, Quaternion.identity);
            UnitCombatHandler handler = go.GetComponent<UnitCombatHandler>();
            handler.IsFrontline = true;
            playerFrontline.Add(handler);
        }

        // Spawn Player Backline
        for (int i = 3; i < 6; i++)
        {
            GameObject unitPrefab = LoadoutData.selectedUnits[i];
            if (unitPrefab == null) continue;

            GameObject go = Instantiate(unitPrefab, playerBackSpawns[i - 3].position, Quaternion.identity);
            UnitCombatHandler handler = go.GetComponent<UnitCombatHandler>();
            handler.IsFrontline = false;
            playerBackline.Add(handler);
        }

        // Spawn Enemy Frontline
        for (int i = 0; i < enemyFrontPrefabs.Count && i < enemyFrontSpawns.Length; i++)
        {
            GameObject go = Instantiate(enemyFrontPrefabs[i], enemyFrontSpawns[i].position, Quaternion.identity);
            UnitCombatHandler handler = go.GetComponent<UnitCombatHandler>();
            handler.IsFrontline = true;
            enemyFrontline.Add(handler);
        }

        // Spawn Enemy Backline
        for (int i = 0; i < enemyBackPrefabs.Count && i < enemyBackSpawns.Length; i++)
        {
            GameObject go = Instantiate(enemyBackPrefabs[i], enemyBackSpawns[i].position, Quaternion.identity);
            UnitCombatHandler handler = go.GetComponent<UnitCombatHandler>();
            handler.IsFrontline = false;
            enemyBackline.Add(handler);
        }

        StartBattle();
    }

    private Transform[] GetChildren(Transform parent)
    {
        Transform[] children = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i);
        }
        return children;
    }

    public void StartBattle()
    {
        turnQueue.Clear();
        turnQueue.AddRange(playerFrontline);
        turnQueue.AddRange(playerBackline);
        turnQueue.AddRange(enemyFrontline);
        turnQueue.AddRange(enemyBackline);

        // Sort based on speed (from UnitStats)
        turnQueue.Sort((a, b) => b.unitStats.GetStats().Speed.CompareTo(a.unitStats.GetStats().Speed));

        currentTurnIndex = 0;
        NextTurn();
    }

    public void NextTurn()
    {
        if (turnQueue.Count == 0) return;

        UnitCombatHandler unit = turnQueue[currentTurnIndex];
        currentTurnIndex = (currentTurnIndex + 1) % turnQueue.Count;
        unit.originalPosition = unit.transform.position;
        unit.StartTurn();
    }

    public UnitCombatHandler PickTarget(UnitCombatHandler attacker)
    {
        List<UnitCombatHandler> enemiesFront;
        List<UnitCombatHandler> enemiesBack;

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

        List<UnitCombatHandler> validFront = enemiesFront.FindAll(u => u.unitStats.GetStats().HP > 0);
        List<UnitCombatHandler> validBack = enemiesBack.FindAll(u => u.unitStats.GetStats().HP > 0);

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
    }
}


