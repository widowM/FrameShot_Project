using UnityEngine;
using DG.Tweening;

public class CameraAnimation : MonoBehaviour
{
    private Material material;
    
    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
            
        DOTween.To(() => material.GetFloat("_t"), 
                   x => material.SetFloat("_t", x), 
                   1f, // target value
                   0.5f) // duration
               .From(-0.021f) // starting value
               .SetEase(Ease.Linear)
               .SetLoops(-1, LoopType.Restart);
    }
}
