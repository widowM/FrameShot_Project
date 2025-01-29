using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private Image fadeInImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fadeInImage.DOFade(0, 2f);
    }
}
