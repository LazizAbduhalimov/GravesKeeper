using System.Collections;
using PoolSystem.Alternative;
using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    [Header("Preview Settings")]
    [SerializeField] protected float PreviewLength = 15f;
    [SerializeField] protected float AttackRate = 2;

    [SerializeField] protected PoolObject MissileObjectPrefab;
    
    [SerializeField, Tooltip("Delay after animation trigger before actual attack")]
    protected float AttackAnimationDelay = 0.3f;
    
    protected PoolMono<PoolObject> MissilesPool;
    protected float PassedAttackTime;
    protected BarController ReloadBar;
    protected Animator animator;
    protected bool isPerformingAttack = false;
    protected PoolMono<PoolObject> LineRendererPool;

    protected virtual void Awake()
    {
        ReloadBar = GetComponentInChildren<BarController>();
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        LineRendererPool = PoolServiceMono.LineRendererPool;
        MissilesPool = PoolServiceMono.PoolService.GetOrRegisterPool(MissileObjectPrefab, 10);
    }
    
    protected LineRenderer GetLineRendererFromPool()
    {
        if (LineRendererPool == null) return null;
        
        var lineObj = LineRendererPool.GetFreeElement(false);
        var lineRenderer = lineObj.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {    
            lineRenderer.gameObject.SetActive(true);
        }
        return lineRenderer;
    }
    
    protected void ReturnLineRendererToPool(LineRenderer lineRenderer)
    {
        if (lineRenderer != null)
        {
            lineRenderer.gameObject.SetActive(false);
        }
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