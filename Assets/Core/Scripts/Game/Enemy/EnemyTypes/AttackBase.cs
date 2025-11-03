using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    [SerializeField] protected float AttackRate = 2;
    protected float PassedAttackTime;
    protected BarController ReloadBar;
    private Animator animator;

    protected virtual void Awake()
    {
        ReloadBar = GetComponentInChildren<BarController>();
        animator = GetComponent<Animator>();
    }

    protected virtual void TryAttack()
    {
        PassedAttackTime += Time.deltaTime;
        if (PassedAttackTime >= AttackRate)
        {
            PassedAttackTime = 0;
            animator?.SetTrigger("Attack");
            Attack();
        }
    }

    protected virtual void RefreshReloadBar()
    {
        ReloadBar.FillAmount = PassedAttackTime / AttackRate;
    }

    protected abstract void Attack();
}