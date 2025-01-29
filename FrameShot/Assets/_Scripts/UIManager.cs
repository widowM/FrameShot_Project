using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject youDiedText;
    [SerializeField] private Text text;
    [Header("Broadcast on Event Channels")]
    [SerializeField] private VoidEventChannelSO enableUIControlsSO;
    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO showGameOverUISO;
    private void ShowGameOverUI()
    {
            startButton.gameObject.SetActive(true);
            youDiedText.SetActive(true);
            startButton.image.DOFade(1f, 1f).SetEase(Ease.Linear);
            text.DOFade(1f, 1f).SetEase(Ease.Linear).OnComplete(() => enableUIControlsSO.RaiseEvent());

    }

    private void OnEnable()
    {
        showGameOverUISO.OnEventRaised += ShowGameOverUI;
    }

    private void OnDisable()
    {
        showGameOverUISO.OnEventRaised -= ShowGameOverUI;
    }

            
}
