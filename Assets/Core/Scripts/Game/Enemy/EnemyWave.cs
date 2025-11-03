using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    public TMP_Text StartText;
    public GameObject Gloom;
    public GameObject GloomLeft;
    public GameObject GloomRight;
    public GameObject Golem;
    public GameObject FireGolem;
    public GameObject Cactus;

    private float _duration = 15f;
    private float _additionalDelay = 2f;
    private bool IsStarted;

    public void Update()
    {
        if (IsStarted) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            StartText.gameObject.SetActive(false);
            IsStarted = true;
            StartCoroutine(SpawnWave());
        }
    }

    public IEnumerator SpawnWave()
    {
        Gloom.SetActive(true);
        yield return new WaitForSeconds(_duration);
        Cactus.SetActive(true);
        yield return new WaitForSeconds(_duration + _additionalDelay);
        Golem.SetActive(true);
        yield return new WaitForSeconds(_duration + _additionalDelay * 2);
        GloomLeft.SetActive(true);
        GloomRight.SetActive(true);
        yield return new WaitForSeconds(_duration + _additionalDelay * 3);
        FireGolem.SetActive(true);
    }
}