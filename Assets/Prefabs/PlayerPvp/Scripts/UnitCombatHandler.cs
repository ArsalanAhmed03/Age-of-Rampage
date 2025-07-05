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

    public bool isLookingTowardsRight = false;
    public Vector3 originalPosition;
    private UnitCombatHandler currentTarget;
    private int finalDamage;
    private bool hasAttacked = false;

    private void Start()
    {
        // originalPosition = transform.position;
        Debug.Log($"{gameObject.name} original position set to {originalPosition}");
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        battleSystem = BattleSystem.Instance;
        // originalPosition = transform.position;

        if (battleSystem == null)
            Debug.LogError("BattleSystem instance not found!");

        UpdateHealthText();
        UpdateSpeedText();

        // Flip player to face towards (0, center of screen) based on isLookingTowardsRight
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        bool isTowardsRight = false;
        if (spriteRenderer != null)
        {
            // If player is to the right of x=0, face left; else face right
            if (transform.position.x > 0)
            {
                isTowardsRight = true;
            }
            else
            {
                isTowardsRight = false;
            }
            spriteRenderer.flipX = isLookingTowardsRight == isTowardsRight;
        }
    }

    public void StartTurn()
    {
        StartAttack();
    }


    public void StartAttack()
    {
        currentTarget = battleSystem.PickTarget(this);
        if (currentTarget == null)
        {
            Debug.Log("No valid target found!");
            return;
        }

        finalDamage = CalculateDamage();
        hasAttacked = false;

        Debug.Log("Original Position: " + originalPosition);

        StartCoroutine(MoveToTarget(currentTarget));
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
        if (HP < 0) HP = 0;
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
        StartCoroutine(ReturnToStart());
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

    public float attackDistance = 0.8f;
    IEnumerator MoveToTarget(UnitCombatHandler target)
    {
        Debug.Log("Original Position: " + originalPosition);
        animator.SetFloat("xVelocity", 1);
        Vector3 targetPos = target.transform.position + Vector3.left * 1f * Mathf.Sign(transform.position.x); // adjust offset if needed

        while (Vector3.Distance(transform.position, targetPos) > attackDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 10f);
            yield return null; // Wait for the next frame
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
        Debug.Log($"[{name}] Starting ReturnToStart. Current: {transform.position}, Target: {originalPosition}");
        animator.SetFloat("xVelocity", 0);

        while (Vector3.Distance(transform.position, originalPosition) > 0f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, Time.deltaTime * 10f);
            Debug.Log($"Moving... {transform.position} -> {originalPosition}");
            yield return null; // Wait for the next frame
        }

        Debug.Log($"{gameObject.name} reached original position.");
        battleSystem.NextTurn();
    }

}
