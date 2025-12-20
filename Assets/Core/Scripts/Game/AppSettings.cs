using UnityEngine;

public class AppSettings : MonoBehaviour
{
    public int TargetFrameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = TargetFrameRate;
    }
}