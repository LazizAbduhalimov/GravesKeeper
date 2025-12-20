using UnityEngine;

public class Enenmy : MonoBehaviour
{
    public int DeathScore = 100;
    private HealthCompponent _helath;

    private void OnEnable() 
    {
        _helath.OnDeath += AddScore;
    }
    private void OnDisable()
    {
        _helath.OnDeath -= AddScore;
    }

    public void AddScore()
    {
        Debug.Log("1231");
        var text = $"+{DeathScore}";
        Damages.Instance.SpawnNumber2(text, gameObject.transform.position);
    }
}