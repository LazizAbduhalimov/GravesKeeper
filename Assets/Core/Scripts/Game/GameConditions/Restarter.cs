using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using PrimeTween;

public class Restarter : MonoBehaviour
{
    private bool _canRestart = false;

    private void OnEnable()
    {
        Tween.Delay(0.5f, () => _canRestart = true, useUnscaledTime: true);    
    }

    private void OnDisable()
    {
        _canRestart = false;
    }

    public void Update()
    {
        // Проверяем тач в новой системе инпута
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Restart();
        }
    }
    
    public void Restart()
    {
        if (!_canRestart) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}