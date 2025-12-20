using System;
using Game;
using TMPro;
using UnityEngine;

public class BrickBuilder : MonoBehaviour {
    public TMP_Text skullsLeft;
    public PortableBrick BrickPrefab;
    public PortableBrick PreviewBrick;

    [SerializeField]
    private float _spawnDistance = 1f;
    [Header("Charges")]
    [SerializeField, Min(0)]
    private int _maxCharges = 3;

    // current available charges
    private int _currentCharges;

    private void Start()
    {
        // nothing for now; PreviewBrick can be assigned in the Inspector
        // initialize charges
        _currentCharges = _maxCharges;
        UpdateSkullsText();

        // subscribe to map BrickMb deaths to increase max charges when they are destroyed
        var mapBricks = FindObjectsByType<BrickMb>(FindObjectsSortMode.None);
        foreach (var mb in mapBricks)
        {
            if (mb.TryGetComponent<HealthCompponent>(out var health))
            {
                // when a map brick dies, increase max charges and give player one charge
                health.OnDeath += () => OnMapBrickDestroyed(mb);
            }
        }
    }

    public void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Build();
        // }

        PreviewBuild();
    }

    public void PreviewBuild()
    {
        if (PreviewBrick == null) return;

        Vector3 previewPos = transform.position + transform.forward * _spawnDistance;
        PreviewBrick.transform.position = previewPos;
        PreviewBrick.transform.rotation = transform.rotation;
    }

    public void Build()
    {
        if (BrickPrefab == null)
        {
            Debug.LogWarning("BrickPrefab is not assigned on BrickBuilder.");
            return;
        }

        if (_currentCharges <= 0)
        {
            Debug.Log("No charges available.");
            return;
        }

        SoundManager.Instance.PlayFX(AllSfxSounds.place);
        Vector3 spawnPos = transform.position + transform.forward * _spawnDistance;
        var brick = Instantiate(BrickPrefab, spawnPos, transform.rotation);

        // consume a charge
        _currentCharges--;
        UpdateSkullsText();

        // if the spawned brick has HealthCompponent, subscribe to its death to restore a charge
        if (brick.TryGetComponent<HealthCompponent>(out var healthComp))
        {
            // capture local reference for removal
            Action onDeath = null;
            onDeath = () =>
            {
                // restore one charge, but not exceeding max
                _currentCharges = Mathf.Min(_currentCharges + 1, _maxCharges);
                UpdateSkullsText();
                // unsubscribe
                healthComp.OnDeath -= onDeath;
            };

            healthComp.OnDeath += onDeath;
        }
    }

    private void UpdateSkullsText()
    {
        if (skullsLeft != null)
        {
            skullsLeft.text = $"left: {_currentCharges}";
        }
    }

    private void OnMapBrickDestroyed(BrickMb mb)
    {
        // increase max charges and also give one available charge immediately
        _maxCharges++;
        _currentCharges = Mathf.Min(_currentCharges + 1, _maxCharges);
        UpdateSkullsText();
    }
}