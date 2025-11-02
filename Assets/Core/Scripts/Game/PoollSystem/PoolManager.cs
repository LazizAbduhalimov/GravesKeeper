using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject _objToPool;
    [SerializeField] private int _poolCount;

    [Header("Other Settings")]
    [SerializeField] private Transform _container;

    private readonly Queue<GameObject> _freeObjects = new ();

    private void Start()
    {
        if (_container == null)
        {
            _container = new GameObject(_objToPool.name + "_Container").transform;
        }

        for (int i = 0; i < _poolCount; i++)
        {
            GameObject obj = Instantiate(_objToPool, _container);
            obj.SetActive(false);

            PooledObject pooled = obj.GetComponent<PooledObject>(); 
            if (pooled == null)
            {
                pooled = obj.AddComponent<PooledObject>();
            }
            pooled.Pool = this;


            _freeObjects.Enqueue(obj);
        }
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        GameObject obj;

        if (_freeObjects.Count > 0)
        {
            obj = _freeObjects.Dequeue();
        }
        else
        {
            obj = Instantiate(_objToPool, _container);
            var pooled = obj.AddComponent<PooledObject>();
            pooled.Pool = this;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(_container);
        obj.transform.position = _container.transform.position;
        _freeObjects.Enqueue(obj);
    }
}
