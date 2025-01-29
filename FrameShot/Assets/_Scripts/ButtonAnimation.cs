using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    private Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();
        image.DOFade(0, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
}
