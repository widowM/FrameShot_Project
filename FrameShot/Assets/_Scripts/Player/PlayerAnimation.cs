using UnityEngine;
using DG.Tweening;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public static int ShownCoyoteIndicatorCounter = 0;
    [SerializeField] GameObject coyoteIndicator;
    [SerializeField] private ParticleSystem landingParticleSystem;
    private bool landingParticlePlayed = false;

    [Header("Listen to Event Channels")]
    [SerializeField] private VoidEventChannelSO wentOffLedgeSO;
    [SerializeField] private VoidEventChannelSO isMovingRightSO;
    [SerializeField] private VoidEventChannelSO isMovingLeftSO;
    [SerializeField] private VoidEventChannelSO playerLandingSO;
    [SerializeField] private VoidEventChannelSO gameOverSO;

    public bool LandingParticlePlayed => landingParticlePlayed;
    public Animator Animator => animator;

    // Define animation hashes
    private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
    private static readonly int IsLandingHash = Animator.StringToHash("IsLanding");
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsIdleHash = Animator.StringToHash("IsIdle");

    private void Update()
    {
        if (player.PlayerPhysics.IsGrounded)
        {
            animator.SetBool(IsJumpingHash, false);
            animator.SetBool(IsLandingHash, false);
        }
        else
        {
            animator.SetBool(IsWalkingHash, false);
            animator.SetBool(IsIdleHash, false);
        }
        if (player.PlayerController.JumpPressed && !player.PlayerPhysics.IsGrounded)
        {
            animator.SetBool(IsJumpingHash, true);
        }
        if (player.PlayerMovement.PlayerMove.x != 0 && player.PlayerPhysics.IsGrounded)
        {
            animator.SetBool(IsWalkingHash, true);
            animator.SetBool(IsIdleHash, false);
        }
        else if (player.PlayerMovement.PlayerMove.x == 0 && player.PlayerPhysics.IsGrounded)
        {
            animator.SetBool(IsWalkingHash, false);
            animator.SetBool(IsIdleHash, true);
        }
        if (player.PlayerPhysics.Rb2D.linearVelocityY < 0 && !player.PlayerPhysics.IsGrounded)
        {
            animator.SetBool(IsJumpingHash, false);
            animator.SetBool(IsLandingHash, true);
        }
    }

    private void ShowCoyoteHelperIndicator()
    {
        coyoteIndicator.SetActive(true);
        if (spriteRenderer.flipX)
        {
            coyoteIndicator.transform.localScale = new Vector2(-1, 1);
        }
        else
        {
            coyoteIndicator.transform.localScale = new Vector2(1, 1);
        }
        coyoteIndicator.GetComponent<LineRenderer>().
            DOColor(new Color2(new Color32(64, 64, 64, 255), new Color32(64, 64, 64, 255)),
            new Color2(new Color32(64, 64, 64, 255), new Color32(64, 64, 64, 255)),
            player.PlayerPhysics.CoyoteTime).SetUpdate(true).OnComplete(()
            => coyoteIndicator.SetActive(false));
        ShownCoyoteIndicatorCounter++;
    }

    private void PlayerLanding()
    {
       landingParticleSystem.Play();
    }

    private void SetSpriteToRight()
    {
        spriteRenderer.flipX = false;
    }

    private void SetSpriteToLeft()
    {
        spriteRenderer.flipX = true;
    }

    private void DisableSpriteRenderer()
    {
        spriteRenderer.enabled = false;
    }

    private void OnEnable()
    {
        wentOffLedgeSO.OnEventRaised += ShowCoyoteHelperIndicator;
        isMovingRightSO.OnEventRaised += SetSpriteToRight;
        isMovingLeftSO.OnEventRaised += SetSpriteToLeft;
        playerLandingSO.OnEventRaised += PlayerLanding;
        gameOverSO.OnEventRaised += DisableSpriteRenderer;
    }

    private void OnDisable()
    {
        wentOffLedgeSO.OnEventRaised -= ShowCoyoteHelperIndicator;
        isMovingRightSO.OnEventRaised -= SetSpriteToRight;
        isMovingLeftSO.OnEventRaised -= SetSpriteToLeft;
        playerLandingSO.OnEventRaised -= PlayerLanding;
        gameOverSO.OnEventRaised -= DisableSpriteRenderer;
    }
}
