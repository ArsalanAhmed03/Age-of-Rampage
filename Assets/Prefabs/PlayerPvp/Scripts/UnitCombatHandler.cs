using System.Collections;
using UnityEngine;
using TMPro;

public class UnitCombatHandler : MonoBehaviour
{
    public bool IsFrontline = true;
    public Animator animator;

    [Header("UI")]
    public TMP_Text healthText;
    public TMP_Text speedText;

    private BattleSystem battleSystem;
    private UnitCombatHandler currentTarget;
    public UnitStats unitStats;
    private int finalDamage;
    private bool hasAttacked = false;

    public bool isLookingTowardsRight = false;
    public Vector3 originalPosition;
    public float attackDistance = 0.8f;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        battleSystem = BattleSystem.Instance;
        unitStats = GetComponent<UnitStats>();

        if (unitStats == null)
            Debug.LogError($"UnitStats missing on {name}");

        if (battleSystem == null)
            Debug.LogError("BattleSystem instance not found!");

        // Flip sprite based on position
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            bool isTowardsRight = transform.position.x > 0;
            spriteRenderer.flipX = isLookingTowardsRight == isTowardsRight;
        }

        UpdateHealthText();
        UpdateSpeedText();
    }

    private void Start()
    {
        UpdateHealthText();
        UpdateSpeedText();
    }

    public void StartTurn()
    {
        StartAttack();
    }

    public void StartAttack()
    {
        Debug.Log($"{gameObject.name} is starting an attack!");

        currentTarget = battleSystem.PickTarget(this);
        if (currentTarget == null)
        {
            Debug.Log("No valid target found!");
            return;
        }

        finalDamage = CalculateDamage();
        hasAttacked = false;

        StartCoroutine(MoveToTarget(currentTarget));
    }

    private int CalculateDamage()
    {
        UnitStatsData stats = unitStats.GetStats();
        float critRoll = Random.Range(0f, 100f);
        bool isCrit = critRoll <= stats.CritChance;

        int damage = isCrit
            ? Mathf.RoundToInt(stats.Attack * stats.CritMultiplier)
            : stats.Attack;

        Debug.Log($"Attack Damage: {stats.Attack}, Critical Hit: {isCrit}, Damage: {damage}");

        if (isCrit)
            Debug.Log($"{gameObject.name} landed a CRITICAL HIT! Damage: {damage}");

        return damage;
    }

    public void TakeDamage(int incomingDamage, UnitCombatHandler attacker)
    {
        float dodgeRoll = Random.Range(0f, 100f);
        float dodgeChance = unitStats.GetStats().DodgeChance;

        if (dodgeRoll <= dodgeChance)
        {
            Debug.Log($"{gameObject.name} DODGED the attack from {attacker.gameObject.name}!");
            return;
        }

        unitStats.baseHP -= incomingDamage;
        if (unitStats.baseHP < 0) unitStats.baseHP = 0;

        UpdateHealthText();

        Debug.Log($"{gameObject.name} took {incomingDamage} damage from {attacker.gameObject.name}. Remaining HP: {unitStats.baseHP}");

        if (unitStats.baseHP <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        battleSystem.RemoveUnit(this);
    }

    public void EndAttack()
    {
        animator.SetBool("isAttacking", false);
        StartCoroutine(ReturnToStart());
    }

    IEnumerator PauseBeforeNextTurn()
    {
        yield return new WaitForSeconds(0.5f);
        battleSystem.NextTurn();
    }

    public void UpdateHealthText()
    {
        if (healthText != null)
            healthText.text = $"HP: {unitStats.GetStats().HP}";
    }

    public void UpdateSpeedText()
    {
        if (speedText != null)
            speedText.text = $"Speed: {unitStats.GetStats().Speed}";
    }

    IEnumerator MoveToTarget(UnitCombatHandler target)
    {
        animator.SetFloat("xVelocity", 1);
        Vector3 targetPos = target.transform.position + Vector3.left * 1f * Mathf.Sign(transform.position.x);

        while (Vector3.Distance(transform.position, targetPos) > attackDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 10f);
            yield return null;
        }

        if (!hasAttacked)
        {
            currentTarget.TakeDamage(finalDamage, this);
            hasAttacked = true;
            animator.SetBool("isAttacking", true);
        }
    }

    IEnumerator ReturnToStart()
    {
        animator.SetFloat("xVelocity", 0);

        while (Vector3.Distance(transform.position, originalPosition) > 0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, Time.deltaTime * 10f);
            yield return null;
        }

        battleSystem.NextTurn();
    }
}
