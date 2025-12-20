using UnityEngine;

public class BrickMb : MonoBehaviour
{
    public GameObject Default;
    public GameObject Destroyed;

    public void OnDisable()
    {
        SoundManager.Instance.PlayOneShotFX(AllSfxSounds.destroy);
        Damages.Instance.SpawnNumber2("+skull", transform.position);
    }
}