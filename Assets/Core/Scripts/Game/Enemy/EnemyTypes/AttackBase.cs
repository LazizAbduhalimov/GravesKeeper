using System.Collections;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    [SerializeField] protected float AttackRate = 2;
    [SerializeField, Tooltip("Delay after animation trigger before actual attack")]
    protected float AttackAnimationDelay = 0.3f;
    
    protected float PassedAttackTime;
    protected BarController ReloadBar;
    protected Animator animator;
    protected bool isPerformingAttack = false;

    protected virtual void Awake()
    {
        ReloadBar = GetComponentInChildren<BarController>();
        animator = GetComponent<Animator>();
    }

    protected virtual void TryAttack()
    {
        if (isPerformingAttack) return;
        
        if (PassedAttackTime >= AttackRate)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }

    protected virtual IEnumerator PerformAttackSequence()
    {
        isPerformingAttack = true;
        PassedAttackTime = 0;
        
        // Play animation
        animator?.SetTrigger("Attack");
        
        // Wait for animation
        yield return new WaitForSeconds(AttackAnimationDelay);
        
        // Perform actual attack
        Attack();
        
        isPerformingAttack = false;
    }

    protected virtual void RefreshReloadBar()
    {
        ReloadBar.FillAmount = PassedAttackTime / AttackRate;
    }

    protected abstract void Attack();
    
    // Call this from Animation Event if you want precise timing
    public void OnAttackAnimationEvent()
    {
        if (isPerformingAttack)
        {
            StopAllCoroutines();
            Attack();
            isPerformingAttack = false;
        }
    }
}