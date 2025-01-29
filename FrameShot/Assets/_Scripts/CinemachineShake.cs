using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// A class responsible for camera shake effects.
/// </summary>
public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }
    private CinemachineCamera _cinemachineVirtualCamera;
    private float _shakeTimer = 0;
    private float _startingIntensity;
    private float _shakeTimerTotal;
    private float intensity = 3;
    private float duration = 0.3f;
    [SerializeField] private VoidEventChannelSO _shakeCamera;


    private void Awake()
    {
        Instance = this;
        _cinemachineVirtualCamera = GetComponent<CinemachineCamera>();

        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            _cinemachineVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0;
    }

    void Update()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.unscaledDeltaTime;

            if (_shakeTimer <= 0f)
            {
                // Time over!
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    _cinemachineVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

                cinemachineBasicMultiChannelPerlin.AmplitudeGain =
                    Mathf.Lerp(_startingIntensity, 0f, 1 - _shakeTimer / _shakeTimerTotal);
            }
        }
    }

    private void ShakeCamera()
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            _cinemachineVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        _startingIntensity = intensity;
        _shakeTimerTotal = duration;
        _shakeTimer = duration;
    }

    private void OnEnable()
    {
        _shakeCamera.OnEventRaised += ShakeCamera;
    }
    private void OnDisable()
    {
        _shakeCamera.OnEventRaised -= ShakeCamera;
    }
}
