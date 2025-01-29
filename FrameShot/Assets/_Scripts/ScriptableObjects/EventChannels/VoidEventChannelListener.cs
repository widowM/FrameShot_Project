using System.Collections;
using UnityEngine;
using UnityEngine.Events;


    /// <summary>
    /// This runs a UnityEvent in response to receiving a specific event channel.
    /// Use it as a simple means of creating codeless interactivity.
    /// </summary>
    public class VoidEventChannelListener : MonoBehaviour
    {
        [Header("Listen to Event Channels")]
        [Tooltip("The signal to listen for")]
        [SerializeField] private VoidEventChannelSO _eventChannelSO;
        [Space]
        [Tooltip("Responds to receiving signal from Event Channel")]
        [SerializeField] private UnityEvent _response;
        [SerializeField] private float _delayInSeconds;

        private void OnEnable()
        {
            if (_eventChannelSO != null)
                _eventChannelSO.OnEventRaised += OnEventRaised;
        }

        private void OnDisable()
        {
            if (_eventChannelSO != null)
                _eventChannelSO.OnEventRaised -= OnEventRaised;
        }

        // Raises an event after a delay
        public void OnEventRaised()
        {
            StartCoroutine(RaiseEventDelayedCoroutine(_delayInSeconds));
        }

        private IEnumerator RaiseEventDelayedCoroutine(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            _response.Invoke();
        }
    }