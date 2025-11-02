using System;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public PoolManager Pool;

    private void OnDisable()
    {
        if(Pool != null) Pool.ReturnToPool(gameObject);
    }
}
