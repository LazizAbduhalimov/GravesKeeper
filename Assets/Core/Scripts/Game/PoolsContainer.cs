using PoolSystem.Alternative;
using UnityEngine;

public class AllPools : MonoBehaviour
{
    public static AllPools Instance => _allPools;
    public static AllPools _allPools;
    
    public PoolContainer ReaperPool;
    public PoolContainer RockPool;
    public PoolContainer CactusPool;
    public PoolContainer LavaPool;
    public PoolContainer LinesPool;

    public void Awake()
    {
        _allPools = this;
    }
}