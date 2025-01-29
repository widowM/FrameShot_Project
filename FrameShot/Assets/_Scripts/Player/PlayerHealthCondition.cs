using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayerHealthCondition : MonoBehaviour
{
    [SerializeField] private Player player;
    private bool hasDied = false;
    [Header("Broadcast On Event Channels")]
    [SerializeField] private VoidEventChannelSO gameOverSO;
    [SerializeField] private VoidEventChannelSO goreStartedSO;
    [SerializeField] private VoidEventChannelSO shakeCameraSO;
    [SerializeField] private VoidEventChannelSO showGameOverUISO;
    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO playerDamagedSO;


    public bool HasDied => hasDied;

    private void StartDeathSequence()
    {
        StartCoroutine(DeathSequenceCoroutine());
    }

    IEnumerator DeathSequenceCoroutine()
    {
        if (!hasDied)
        {
            gameOverSO.RaiseEvent();
            hasDied = true;
  
            yield return StartCoroutine(FreezeTimeCoroutine());
            goreStartedSO.RaiseEvent();

            shakeCameraSO.RaiseEvent();

            yield return new WaitForSecondsRealtime(1f);
            yield return new WaitForSecondsRealtime(1);
            Time.timeScale = 1;
            showGameOverUISO.RaiseEvent();
        
        }
    }

    private IEnumerator FreezeTimeCoroutine()
    {
        var freezeTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, 0.0001f).SetUpdate(true);
        var delayTween = DOVirtual.DelayedCall(0.45f, () =>
        {
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1.5f, 0.0001f).SetUpdate(true);
        }).SetUpdate(true);
        yield return delayTween.WaitForCompletion();
    }

    private void OnEnable()
    {
        playerDamagedSO.OnEventRaised += StartDeathSequence;
    }

    private void OnDisable()
    {
        playerDamagedSO.OnEventRaised -= StartDeathSequence;
    }
}
