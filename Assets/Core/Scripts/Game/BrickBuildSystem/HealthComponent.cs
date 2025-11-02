using Unity.Collections;
using UnityEngine;

public class HealthCompponent : MonoBehaviour
{
    public int MaxHealth;
    [ReadOnly] private int _currentHealth;

    public void TakeOneDamage()
    {
        _currentHealth--;

        if (_currentHealth < 1)
        {
            Die();
        }
    } 
    
    public void Die()
    {
        gameObject.SetActive(false);
    }
}