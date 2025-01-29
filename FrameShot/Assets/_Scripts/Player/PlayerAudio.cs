using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource stepAudioSource;
    [SerializeField] private AudioClip cameraShutterSFX;
    [SerializeField] private AudioClip frameMoveSFX;
    [SerializeField] private AudioClip earthRumbleSFX;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip landingSFX;
    [SerializeField] private AudioClip goreSFX;
    [SerializeField] private AudioClip gameOverSFX;
    [SerializeField] private AudioClip negatePlaceCopySFX;
    [SerializeField] private AudioClip[] stepsSFX;
    private bool playedLandingSound = false;

    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO playSnapshotSoundSO;
    [SerializeField] private VoidEventChannelSO cameraCollectedSO;
    [SerializeField] private VoidEventChannelSO snapshotNegatedSoundSO;
    [SerializeField] private VoidEventChannelSO copyPastedSO;
    [SerializeField] private VoidEventChannelSO playerLandedSO;
    [SerializeField] private VoidEventChannelSO playerJumpedSO;
    [SerializeField] private VoidEventChannelSO frameMovedSO;
    [SerializeField] private VoidEventChannelSO showGameOverUISO;
    [SerializeField] private VoidEventChannelSO prepareLandinSoundSO;
    [SerializeField] private VoidEventChannelSO goreStartedSO;

    public bool PlayedLandingSound => playedLandingSound;
    private void PlaySnapshotSound()
    {
        audioSource.PlayOneShot(cameraShutterSFX, 2);
    }

    private void PlaySnapshotNegationSound()
    {
        audioSource.PlayOneShot(negatePlaceCopySFX, 0.3f);
    }

    private void PlayPasteSound()
    {
        audioSource.PlayOneShot(earthRumbleSFX, 1f);
    }

    private void PlayLandingSound()
    {
        if (!playedLandingSound)
        {
            audioSource.pitch = Random.Range(0.8f, 1f);
            audioSource.PlayOneShot(landingSFX, 0.3f);
            playedLandingSound = true;
        }
    }

    private void PlayJumpingSound()
    {
        if (!player.PlayerPhysics.IsGrounded) return;
        audioSource.pitch = Random.Range(0.8f, 1f);
        audioSource.PlayOneShot(jumpSFX, 1.5f);
        playedLandingSound = false;
    }

    private void PlayFrameMoveSFX()
    {
        audioSource.PlayOneShot(frameMoveSFX, 0.3f);
    }

    public void PlayStepSound()
    {
        if (player.PlayerHealthCondition.HasDied) return;
        if (stepAudioSource.isPlaying) return;
        stepAudioSource.pitch = Random.Range(0.7f, 1f);
        stepAudioSource.PlayOneShot(stepsSFX[Random.Range(0, stepsSFX.Length)], 1.5f);
    }

    private void PlayGoreSound()
    {
        audioSource.PlayOneShot(goreSFX, 1f);
    }

    private void PlayGameOverSound()
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(gameOverSFX, 1f);
    }

    private void PrepareLandingSound()
    {
        playedLandingSound = false;
    }

    private void OnEnable()
    {
        playSnapshotSoundSO.OnEventRaised += PlaySnapshotSound;
        cameraCollectedSO.OnEventRaised += PlaySnapshotSound;
        snapshotNegatedSoundSO.OnEventRaised += PlaySnapshotNegationSound;
        copyPastedSO.OnEventRaised += PlayPasteSound;
        playerLandedSO.OnEventRaised += PlayLandingSound;
        playerJumpedSO.OnEventRaised += PlayJumpingSound;
        frameMovedSO.OnEventRaised += PlayFrameMoveSFX;
        showGameOverUISO.OnEventRaised += PlayGameOverSound;
        prepareLandinSoundSO.OnEventRaised += PrepareLandingSound;
        goreStartedSO.OnEventRaised += PlayGoreSound;
    }

    private void OnDisable()
    {
        playSnapshotSoundSO.OnEventRaised -= PlaySnapshotSound;
        cameraCollectedSO.OnEventRaised -= PlaySnapshotSound;
        snapshotNegatedSoundSO.OnEventRaised -= PlaySnapshotNegationSound;
        copyPastedSO.OnEventRaised -= PlayPasteSound;
        playerLandedSO.OnEventRaised -= PlayLandingSound;
        playerJumpedSO.OnEventRaised -= PlayJumpingSound;
        frameMovedSO.OnEventRaised -= PlayFrameMoveSFX;
        goreStartedSO.OnEventRaised -= PlayGoreSound;
        showGameOverUISO.OnEventRaised -= PlayGameOverSound;
        prepareLandinSoundSO.OnEventRaised -= PrepareLandingSound;
    }
}
