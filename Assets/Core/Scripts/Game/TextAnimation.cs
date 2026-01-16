using UnityEngine;
using PrimeTween;
using TMPro;


[RequireComponent(typeof(TMP_Text))]
public class TextAnimation : MonoBehaviour
{
    private TMP_Text tMP_Text;

    Sequence? _currentTween;

    private void Awake()
    {
        tMP_Text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        tMP_Text.alpha = 0;
        _currentTween = Sequence.Create(useUnscaledTime: true, cycles: int.MaxValue)
            .Chain(Tween.Alpha(tMP_Text, 1f, 1f, Ease.Linear))
            .Chain(Tween.Delay(2))
            .Chain(Tween.Alpha(tMP_Text, 0f, 1f, Ease.Linear));
    }

    private void OnDisable()
    {
        _currentTween?.Complete();
    }
}