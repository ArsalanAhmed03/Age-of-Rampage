using System.Collections;
using UnityEngine;
using TMPro;

public class UnitCombatHandler : MonoBehaviour
{
    [Header("Stats")]
    public int HP = 100;
    public int AttackDamage = 20;
    public int Speed = 10;
    [Range(0, 100)] public float CriticalChance = 10f;  // % chance for critical hit
    public float CriticalDamageMultiplier = 1.5f;       // 1.5x by default
    [Range(0, 100)] public float DodgeChance = 5f;       // % chance to dodge

    public bool IsFrontline = true;
    public Animator animator;

    [Header("UI")]
    public TMP_Text healthText;
    public TMP_Text speedText;

    private BattleSystem battleSystem;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        battleSystem = FindFirstObjectByType<BattleSystem>();
        UpdateHealthText();
        UpdateSpeedText();
    }

    public void StartTurn()
    {
        StartAttack();
    }

    public void StartAttack()
    {
        UnitCombatHandler target = battleSystem.PickTarget(this);

        if (target == null)
        {
            Debug.Log("No valid target found!");
            return;
        }

        animator.SetBool("isAttacking", true);

        // Calculate actual damage based on critical
        int finalDamage = CalculateDamage();

        // Apply damage to target
        target.TakeDamage(finalDamage, this);
    }

    private int CalculateDamage()
    {
        float critRoll = Random.Range(0f, 100f);
        bool isCrit = critRoll <= CriticalChance;

        int damage = isCrit
            ? Mathf.RoundToInt(AttackDamage * CriticalDamageMultiplier)
            : AttackDamage;

        if (isCrit)
            Debug.Log($"{gameObject.name} landed a CRITICAL HIT! Damage: {damage}");

        return damage;
    }

    public void TakeDamage(int incomingDamage, UnitCombatHandler attacker)
    {
        float dodgeRoll = Random.Range(0f, 100f);
        if (dodgeRoll <= DodgeChance)
        {
            Debug.Log($"{gameObject.name} DODGED the attack from {attacker.gameObject.name}!");
            return;
        }

        HP -= incomingDamage;
        UpdateHealthText();

        if (HP > 0)
        {
            Debug.Log($"{gameObject.name} took {incomingDamage} damage from {attacker.gameObject.name}. Remaining HP: {HP}");
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        battleSystem.RemoveUnit(this);
    }

    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
        Debug.Log("EndAttack: Reset isAttacking.");
        StartCoroutine(PauseBeforeNextTurn());
    }

    IEnumerator PauseBeforeNextTurn()
    {
        yield return new WaitForSeconds(0.5f);
        battleSystem.NextTurn();
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = $"HP: {HP}";
    }

    private void UpdateSpeedText()
    {
        if (speedText != null)
            speedText.text = $"Speed: {Speed}";
    }
}
