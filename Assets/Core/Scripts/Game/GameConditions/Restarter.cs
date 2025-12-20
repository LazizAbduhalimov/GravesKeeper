using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Restarter : MonoBehaviour
{
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}