using PoolSystem.Alternative;
using UnityEngine;

public class PoolServiceMono : MonoBehaviour
{
    public static PoolService PoolService;
    public static PoolMono<PoolObject> LineRendererPool;

    public PoolObject LineRendererPrefab;

    public void Awake()
    {
        PoolService = new PoolService("Pools");
        LineRendererPool = PoolService.GetOrRegisterPool(LineRendererPrefab, 10);    
    }
}